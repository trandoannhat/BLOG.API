using NhatSoft.Application.DTOs.Donation;

namespace NhatSoft.Application.Interfaces;

public interface IDonationService
{
    Task<DonationResponseDto> CreateDonationAsync(CreateDonationDto request);

    // Chuẩn pattern trả về Tuple để Controller ném vào PagedResponse
    Task<(IEnumerable<DonationResponseDto> Data, int TotalRecords)> GetPagedDonationsAsync(DonationFilterParams filter);

    // Hàm đặc thù cho giao diện chạy chữ ngoài trang chủ web
    Task<IEnumerable<DonationResponseDto>> GetLatestApprovedDonationsAsync(int limit = 20);

    Task ToggleApprovalAsync(Guid id);

    Task DeleteDonationAsync(Guid id);

    //mục tiêu donate

    Task<DonationStatsDto> GetDonationStatsAsync();
}
