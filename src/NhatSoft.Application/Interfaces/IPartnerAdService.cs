using NhatSoft.Application.DTOs.PartnerAd;
using NhatSoft.Domain.Enums;

namespace NhatSoft.Application.Interfaces;

public interface IPartnerAdService
{
    Task<IEnumerable<PartnerAdDto>> GetActiveAdsByPositionAsync(AdPosition position);
    Task<IEnumerable<PartnerAdDto>> GetAllAsync();
    Task<PartnerAdDto> GetByIdAsync(Guid id);
    Task<PartnerAdDto> CreateAsync(CreatePartnerAdDto request);
    Task UpdateAsync(UpdatePartnerAdDto request);
    Task DeleteAsync(Guid id);
    Task ToggleActiveAsync(Guid id);
}