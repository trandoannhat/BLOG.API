using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Account;
using NhatSoft.Application.Interfaces;
using System.Security.Claims;

namespace NhatSoft.API.Controllers
{
    // Đừng quên 2 thẻ này (nếu BaseApiController chưa có)
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService accountService) : BaseApiController
    {
        // ==========================================
        // 1. PUBLIC ENDPOINTS (Không cần Token)
        // ==========================================

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginRequest request)
        {
            var response = await accountService.AuthenticateAsync(request);

            if (!response.Success)
            {
                // Nếu bạn có hàm Error trong BaseApiController
                return Error(response.Message, "AuthFailed");
                // Hoặc giữ nguyên BadRequest(response) nếu response đã chuẩn JSON bạn muốn
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var response = await accountService.RegisterAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // ==========================================
        // 2. PROTECTED ENDPOINTS (Bắt buộc có Token)
        // ==========================================

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            //  Đã đồng bộ cách lấy ID an toàn nhất
            var userId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await accountService.GetProfileAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            //  Đã đồng bộ cách lấy ID an toàn nhất
            var userId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await accountService.UpdateProfileAsync(userId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}