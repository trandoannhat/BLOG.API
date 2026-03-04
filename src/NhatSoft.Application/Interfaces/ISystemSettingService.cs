using NhatSoft.Application.DTOs.SystemSetting;

namespace NhatSoft.Application.Interfaces;

public interface ISystemSettingService
{
    Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync();
    Task<bool> UpdateBatchSettingsAsync(IEnumerable<UpdateSettingRequest> requests);
}
