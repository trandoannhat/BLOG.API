using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Account;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Constants;
using NhatSoft.Domain.Enums;
using System.Security.Claims;

namespace NhatSoft.API.Controllers
{
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
            if (!response.Success) return Error(response.Message, "AuthFailed");

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            var response = await accountService.RegisterAsync(request);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        // ==========================================
        // 2. PROTECTED ENDPOINTS (User tự quản lý hồ sơ)
        // ==========================================

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await accountService.GetProfileAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await accountService.UpdateProfileAsync(userId, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        // ==========================================
        // 3. ADMIN ENDPOINTS (Chỉ Admin)
        // ==========================================

        [HttpGet("users")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await accountService.GetAllUsersAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UserRole newRole)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID không hợp lệ");

            // Lấy ID của Admin đang đăng nhập thực hiện thao tác này
            var currentUserId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Truyền cả ID người thao tác và ID mục tiêu
            var response = await accountService.UpdateUserRoleAsync(currentUserId, id, newRole);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID không hợp lệ");

            // Lấy ID của Admin đang thực hiện thao tác xóa
            var currentUserId = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Truyền cả ID người thao tác và ID mục tiêu bị xóa
            var response = await accountService.DeleteUserAsync(currentUserId, id);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}