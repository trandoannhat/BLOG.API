using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.DTOs.PartnerAd;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions; // 👈 Dùng chung thư viện ném Exception của bạn
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Enums;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

public class PartnerAdService(IUnitOfWork unitOfWork, IMapper mapper) : IPartnerAdService
{
    // 1. PUBLIC
    public async Task<IEnumerable<PartnerAdDto>> GetActiveAdsByPositionAsync(AdPosition position)
    {
        var ads = await unitOfWork.PartnerAds.GetAllQueryable()
            .Where(x => x.IsActive && x.Position == position)
            .OrderByDescending(x => x.CreatedAt) // Sắp xếp theo BaseEntity
            .ToListAsync();

        return mapper.Map<IEnumerable<PartnerAdDto>>(ads);
    }

    // 2. GET ALL
    public async Task<IEnumerable<PartnerAdDto>> GetAllAsync()
    {
        var ads = await unitOfWork.PartnerAds.GetAllQueryable()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return mapper.Map<IEnumerable<PartnerAdDto>>(ads);
    }

    // 3. GET BY ID
    public async Task<PartnerAdDto> GetByIdAsync(Guid id)
    {
        var ad = await unitOfWork.PartnerAds.GetByIdAsync(id);

        // Ném lỗi y như PostService
        if (ad == null) throw new NotFoundException("Không tìm thấy quảng cáo");

        return mapper.Map<PartnerAdDto>(ad);
    }

    // 4. CREATE
    public async Task<PartnerAdDto> CreateAsync(CreatePartnerAdDto request)
    {
        var ad = mapper.Map<PartnerAd>(request);

        await unitOfWork.PartnerAds.AddAsync(ad);
        await unitOfWork.CompleteAsync();

        return mapper.Map<PartnerAdDto>(ad);
    }

    // 5. UPDATE
    public async Task UpdateAsync(UpdatePartnerAdDto request)
    {
        var ad = await unitOfWork.PartnerAds.GetByIdAsync(request.Id);
        if (ad == null) throw new NotFoundException("Không tìm thấy quảng cáo");

        mapper.Map(request, ad);

        unitOfWork.PartnerAds.Update(ad);
        await unitOfWork.CompleteAsync();
    }

    // 6. DELETE
    public async Task DeleteAsync(Guid id)
    {
        var ad = await unitOfWork.PartnerAds.GetByIdAsync(id);
        if (ad == null) throw new NotFoundException("Không tìm thấy quảng cáo");

        unitOfWork.PartnerAds.Delete(ad);
        await unitOfWork.CompleteAsync();
    }

    // 7. TOGGLE
    public async Task ToggleActiveAsync(Guid id)
    {
        var ad = await unitOfWork.PartnerAds.GetByIdAsync(id);
        if (ad == null) throw new NotFoundException("Không tìm thấy quảng cáo");

        ad.IsActive = !ad.IsActive;
        unitOfWork.PartnerAds.Update(ad);
        await unitOfWork.CompleteAsync();
    }
}