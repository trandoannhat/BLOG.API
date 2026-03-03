using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.DTOs.Donation;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

public class DonationService(IUnitOfWork unitOfWork, IMapper mapper) : IDonationService
{
    public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationDto request)
    {
        var donation = mapper.Map<Donation>(request);

        // Mặc định khách gửi lên là chưa duyệt
        donation.IsConfirmed = false;

        await unitOfWork.Donations.AddAsync(donation);
        await unitOfWork.CompleteAsync();

        return mapper.Map<DonationResponseDto>(donation);
    }

    public async Task<(IEnumerable<DonationResponseDto> Data, int TotalRecords)> GetPagedDonationsAsync(DonationFilterParams filter)
    {
        var query = unitOfWork.Donations.GetAllQueryable().AsNoTracking();

        // Xử lý tìm kiếm bằng Keyword (Thuộc tính kế thừa từ PaginationFilter)
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(d =>
                (d.DonorName != null && d.DonorName.ToLower().Contains(keyword)) ||
                (d.Message != null && d.Message.ToLower().Contains(keyword)) ||
                (d.PaymentMethod != null && d.PaymentMethod.ToLower().Contains(keyword))
            );
        }

        // Xử lý lọc theo trạng thái
        if (filter.IsConfirmed.HasValue)
        {
            query = query.Where(d => d.IsConfirmed == filter.IsConfirmed.Value);
        }

        // Đếm tổng số lượng (Phục vụ phân trang)
        var totalRecords = await query.CountAsync();

        // Cắt dữ liệu (Skip, Take dùng PageNumber, PageSize kế thừa từ PaginationFilter)
        var pagedDonations = await query
                            .OrderByDescending(d => d.CreatedAt)
                            .Skip((filter.PageNumber - 1) * filter.PageSize)
                            .Take(filter.PageSize)
                            .ToListAsync();

        var resultDto = mapper.Map<IEnumerable<DonationResponseDto>>(pagedDonations);

        return (resultDto, totalRecords);
    }

    public async Task<IEnumerable<DonationResponseDto>> GetLatestApprovedDonationsAsync(int limit = 20)
    {
        var donations = await unitOfWork.Donations.GetAllQueryable()
                                     .AsNoTracking()
                                     .Where(d => d.IsConfirmed)
                                     .OrderByDescending(d => d.CreatedAt)
                                     .Take(limit)
                                     .ToListAsync();

        return mapper.Map<IEnumerable<DonationResponseDto>>(donations);
    }

    public async Task ToggleApprovalAsync(Guid id)
    {
        var donation = await unitOfWork.Donations.GetByIdAsync(id);
        if (donation == null)
            throw new NotFoundException("Không tìm thấy giao dịch ủng hộ");

        // Đảo ngược trạng thái
        donation.IsConfirmed = !donation.IsConfirmed;

        unitOfWork.Donations.Update(donation);
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteDonationAsync(Guid id)
    {
        var donation = await unitOfWork.Donations.GetByIdAsync(id);
        if (donation == null)
            throw new NotFoundException("Không tìm thấy giao dịch ủng hộ");

        unitOfWork.Donations.Delete(donation);
        await unitOfWork.CompleteAsync();
    }

    public async Task<DonationStatsDto> GetDonationStatsAsync()
    {
        // 1. Lấy tổng tiền và danh sách (Đã duyệt)
        var approvedDonations = unitOfWork.Donations.GetAllQueryable()
                                          .AsNoTracking()
                                          .Where(d => d.IsConfirmed);

        var totalRaised = await approvedDonations.SumAsync(d => d.Amount);

        // 2. Tìm Top Supporter
        var topSupporters = await approvedDonations
            .GroupBy(d => d.DonorName)
            .Select(g => new TopSupporterDto
            {
                DonorName = string.IsNullOrEmpty(g.Key) ? "Ẩn danh" : g.Key,
                TotalAmount = g.Sum(d => d.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(5) // Lấy Top 5
            .ToListAsync();

        // 3. Đọc TargetAmount từ DB (Bảng SystemSettings)
        var targetSetting = await unitOfWork.SystemSettings.GetAllQueryable()
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(s => s.Key == "DonationTarget");

        decimal targetAmount = 2000000; // Giá trị mặc định nếu trong DB chưa có
        if (targetSetting != null && decimal.TryParse(targetSetting.Value, out var parsedValue))
        {
            targetAmount = parsedValue;
        }

        return new DonationStatsDto
        {
            TotalRaised = totalRaised,
            TargetAmount = targetAmount, // Đã lấy từ DB linh hoạt
            TopSupporters = topSupporters
        };
    }

    // --- HÀM MỚI: DÙNG ĐỂ ADMIN LƯU MỤC TIÊU MỚI ---
    public async Task<bool> UpdateDonationTargetAsync(decimal newTarget)
    {
        var targetSetting = await unitOfWork.SystemSettings.GetAllQueryable()
                                            .FirstOrDefaultAsync(s => s.Key == "DonationTarget");

        if (targetSetting == null)
        {
            // Nếu chưa tồn tại trong DB thì tạo mới
            targetSetting = new SystemSetting
            {
                Key = "DonationTarget",
                Value = newTarget.ToString(),
                Description = "Mục tiêu donate duy trì Server (VNĐ)"
            };
            await unitOfWork.SystemSettings.AddAsync(targetSetting);
        }
        else
        {
            // Nếu có rồi thì cập nhật Value
            targetSetting.Value = newTarget.ToString();
            unitOfWork.SystemSettings.Update(targetSetting);
        }

        // Gọi CompleteAsync() thay vì SaveChangesAsync() theo chuẩn IUnitOfWork của bạn
        return await unitOfWork.CompleteAsync() > 0;
    }
}