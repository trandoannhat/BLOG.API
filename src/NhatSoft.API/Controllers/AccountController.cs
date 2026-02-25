using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Account;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.API.Controllers
{
    public class AccountController(IAccountService accountService) : BaseApiController
    {
        // 1. Đăng nhập
        // URL: POST /api/Account/authenticate
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(LoginRequest request)
        {
            var response = await accountService.AuthenticateAsync(request);

            // Nếu login thất bại (Succeeded = false) -> Trả về BadRequest (400)
            if (!response.Success)
            {
                return BadRequest(response); // Trả về lỗi kèm message
            }

            // Nếu thành công -> Trả về OK (200) kèm Token
            return Ok(response);
        }

        // 2. Đăng ký
        // URL: POST /api/Account/register
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
    }
}