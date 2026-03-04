using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.SystemSetting;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Constants;
using NhatSoft.Common.Wrappers;

namespace NhatSoft.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppConstants.Roles.Admin)] // Khóa chặt, chỉ Admin mới được vào
public class SystemSettingsController(ISystemSettingService systemSettingService) : ControllerBase
{
    // ==========================================================
    // 1. PUBLIC API (Cho phép Frontend đọc cấu hình để render web)
    // ==========================================================
    [HttpGet]
    [AllowAnonymous] // Cấu hình như FacebookUrl, LogoUrl... ai cũng đọc được
    public async Task<IActionResult> GetAll()
    {
        var data = await systemSettingService.GetAllSettingsAsync();
        return Ok(new ApiResponse<IEnumerable<SystemSettingDto>>(data, "Lấy cấu hình hệ thống thành công."));
    }

    // ==========================================================
    // 2. ADMIN API (Lưu cấu hình hàng loạt)
    // ==========================================================
    [HttpPost("batch")]
    public async Task<IActionResult> UpdateBatch([FromBody] IEnumerable<UpdateSettingRequest> requests)
    {
        if (requests == null || !requests.Any())
        {
            return BadRequest(new ApiResponse<string>(string.Empty, "Không có dữ liệu để cập nhật."));
        }

        var result = await systemSettingService.UpdateBatchSettingsAsync(requests);

        if (result)
        {
            return Ok(new ApiResponse<string>(string.Empty, "Cập nhật cấu hình hệ thống thành công!"));
        }

        return BadRequest(new ApiResponse<string>(string.Empty, "Cập nhật thất bại. Vui lòng thử lại."));
    }
}