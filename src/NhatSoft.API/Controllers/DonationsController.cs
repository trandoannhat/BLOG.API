using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Donation;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Wrappers; // Gọi các class bọc kết quả chuẩn của bạn

namespace NhatSoft.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DonationsController(IDonationService donationService) : ControllerBase
{
    // ==========================================================
    // 1. PUBLIC API (Dành cho Website NhatDev - Khách vãng lai)
    // ==========================================================

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateDonationDto request)
    {
        var result = await donationService.CreateDonationAsync(request);

        // Trả về ApiResponse chuẩn
        return Ok(new ApiResponse<DonationResponseDto>(result, "Gửi thông tin ủng hộ thành công! NhatSoft sẽ duyệt trong ít phút."));
    }

    [HttpGet("approved")]
    [AllowAnonymous]
    public async Task<IActionResult> GetApprovedLatest([FromQuery] int limit = 20)
    {
        var data = await donationService.GetLatestApprovedDonationsAsync(limit);
        return Ok(new ApiResponse<IEnumerable<DonationResponseDto>>(data, "Lấy danh sách hiển thị thành công."));
    }

    // ==========================================================
    // 2. ADMIN API (Dành cho CMS Quản trị - Bắt buộc Đăng nhập)
    // ==========================================================

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPaged([FromQuery] DonationFilterParams filter)
    {
        var (data, totalRecords) = await donationService.GetPagedDonationsAsync(filter);

        // Trả về PagedResponse tự động tính toán TotalPages
        var response = new PagedResponse<IEnumerable<DonationResponseDto>>(
            data,
            filter.PageNumber,
            filter.PageSize,
            totalRecords,
            "Danh sách Donation phân trang"
        );

        return Ok(response);
    }

    [HttpPut("{id}/toggle-approval")]
    [Authorize]
    public async Task<IActionResult> ToggleApproval(Guid id)
    {
        await donationService.ToggleApprovalAsync(id);

        // Truyền rỗng (hoặc null) cho data nếu chỉ cần báo Message thành công
        return Ok(new ApiResponse<string>(string.Empty, "Cập nhật trạng thái duyệt thành công."));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await donationService.DeleteDonationAsync(id);
        return Ok(new ApiResponse<string>(string.Empty, "Xóa thông tin giao dịch thành công."));
    }
}