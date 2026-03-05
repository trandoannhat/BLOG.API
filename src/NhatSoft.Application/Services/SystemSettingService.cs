using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NhatSoft.Application.DTOs.SystemSetting;
using NhatSoft.Application.Interfaces;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

// ĐÃ THÊM: ILogger để bắt lỗi trên production
public class SystemSettingService(IUnitOfWork unitOfWork, ILogger<SystemSettingService> logger) : ISystemSettingService
{
    public async Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync()
    {
        var settings = await unitOfWork.SystemSettings.GetAllQueryable()
                                     .AsNoTracking()
                                     .Select(s => new SystemSettingDto
                                     {
                                         Key = s.Key,
                                         Value = s.Value
                                     })
                                     .ToListAsync();

        return settings;
    }

    public async Task<bool> UpdateBatchSettingsAsync(IEnumerable<UpdateSettingRequest> requests)
    {
        try
        {
            if (requests == null || !requests.Any()) return false;

            // Kéo toàn bộ cấu hình lên và ép sang Dictionary để tra cứu O(1)
            var existingSettings = await unitOfWork.SystemSettings.GetAllQueryable().ToListAsync();
            var settingsDict = existingSettings.ToDictionary(s => s.Key, s => s);

            foreach (var req in requests)
            {
                if (string.IsNullOrEmpty(req.Key)) continue;

                // TỐI ƯU: Dùng TryGetValue thay vì FirstOrDefault
                if (settingsDict.TryGetValue(req.Key, out var setting))
                {
                    if (setting.Value != req.Value)
                    {
                        setting.Value = req.Value ?? "";
                        unitOfWork.SystemSettings.Update(setting);
                    }
                }
                else
                {
                    var newSetting = new SystemSetting
                    {
                        Key = req.Key,
                        Value = req.Value ?? "",
                        Description = "Cấu hình hệ thống tự động cập nhật"
                    };
                    await unitOfWork.SystemSettings.AddAsync(newSetting);
                }
            }

            await unitOfWork.CompleteAsync();
            return true;
        }
        catch (Exception ex)
        {
            // ĐÃ THÊM: Log lỗi để truy vết trên Kestrel/PM2/Docker
            logger.LogError(ex, "[SystemSettingService] Lỗi khi cập nhật cấu hình hàng loạt.");
            return false;
        }
    }
}