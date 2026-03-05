using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Contact;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Constants;
using NhatSoft.Common.Wrappers; // Đảm bảo đã import namespace chứa ApiResponse

namespace NhatSoft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public class ContactsController(IContactService contactService) : ControllerBase
    {
        [HttpPost] // Public: Khách gửi liên hệ từ web Next.js
        [AllowAnonymous]
        public async Task<IActionResult> SendContact(CreateContactDto request)
        {
            await contactService.CreateAsync(request);
            // Bọc bằng ApiResponse
            return Ok(new ApiResponse<string>(string.Empty, "Gửi liên hệ thành công. NhatSoft sẽ phản hồi sớm nhất!"));
        }

        [HttpGet] // Admin: Xem danh sách
        public async Task<IActionResult> GetAll([FromQuery] ContactFilterParams filter)
        {
            var (data, total) = await contactService.GetPagedAsync(filter);

            // Bọc Data và TotalRecords vào một object nặc danh bên trong ApiResponse
            var responseData = new { items = data, total = total };
            return Ok(new ApiResponse<object>(responseData, "Lấy danh sách thành công"));
        }

        [HttpPut("{id}")] // Admin: Cập nhật trạng thái
        public async Task<IActionResult> Update(Guid id, UpdateContactDto request)
        {
            if (id != request.Id)
                return BadRequest(new ApiResponse<string>(string.Empty, "ID không hợp lệ"));

            await contactService.UpdateStatusAsync(request);
            return Ok(new ApiResponse<string>(string.Empty, "Đã cập nhật trạng thái"));
        }

        [HttpDelete("{id}")] // Admin: Xóa
        public async Task<IActionResult> Delete(Guid id)
        {
            await contactService.DeleteAsync(id);
            return Ok(new ApiResponse<string>(string.Empty, "Đã xóa liên hệ"));
        }
    }
}