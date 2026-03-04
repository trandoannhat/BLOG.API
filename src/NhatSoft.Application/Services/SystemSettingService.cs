using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.DTOs.SystemSetting;
using NhatSoft.Application.Interfaces;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

// Dùng Primary Constructor y hệt style của bạn
public class SystemSettingService(IUnitOfWork unitOfWork) : ISystemSettingService
{
    public async Task<IEnumerable<SystemSettingDto>> GetAllSettingsAsync()
    {
        // Lấy tất cả cấu hình từ DB và map sang DTO
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

            // Kéo toàn bộ cấu hình hiện tại lên để kiểm tra
            var existingSettings = await unitOfWork.SystemSettings.GetAllQueryable().ToListAsync();

            foreach (var req in requests)
            {
                // Bỏ qua nếu Key bị trống
                if (string.IsNullOrEmpty(req.Key)) continue;

                var setting = existingSettings.FirstOrDefault(s => s.Key == req.Key);

                if (setting != null)
                {
                    // Nếu giá trị khác thì mới cập nhật (tối ưu hiệu năng)
                    if (setting.Value != req.Value)
                    {
                        setting.Value = req.Value ?? "";
                        unitOfWork.SystemSettings.Update(setting);
                    }
                }
                else
                {
                    // Nếu chưa có Key này trong DB -> Thêm mới
                    var newSetting = new SystemSetting
                    {
                        Key = req.Key,
                        Value = req.Value ?? "",
                        Description = "Cấu hình hệ thống tự động"
                    };
                    await unitOfWork.SystemSettings.AddAsync(newSetting);
                }
            }

            // Lưu tất cả thay đổi
            await unitOfWork.CompleteAsync();

            return true; // Luôn trả về true khi xử lý xong không lỗi
        }
        catch (Exception ex)
        {
            // Bạn có thể log ex ở đây để kiểm tra lỗi cụ thể nếu cần
            return false;
        }
    }
}