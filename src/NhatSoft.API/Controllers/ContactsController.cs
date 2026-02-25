using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Contact;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController(IContactService contactService) : ControllerBase
    {
        [HttpPost] // Public: Khách gửi liên hệ
        public async Task<IActionResult> SendContact(CreateContactDto request)
        {
            await contactService.CreateAsync(request);
            return Ok(new { Message = "Gửi liên hệ thành công" });
        }

        [HttpGet] // Admin: Xem danh sách
        public async Task<IActionResult> GetAll([FromQuery] ContactFilterParams filter)
        {
            var (data, total) = await contactService.GetPagedAsync(filter);
            return Ok(new { Data = data, TotalRecords = total });
        }

        [HttpPut("{id}")] // Admin: Cập nhật trạng thái
        public async Task<IActionResult> Update(Guid id, UpdateContactDto request)
        {
            if (id != request.Id) return BadRequest();
            await contactService.UpdateStatusAsync(request);
            return Ok(new { Message = "Đã cập nhật" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await contactService.DeleteAsync(id);
            return Ok(new { Message = "Đã xóa" });
        }
    }
}
