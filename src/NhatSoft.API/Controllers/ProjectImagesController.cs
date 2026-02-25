using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectImagesController(IProjectImageService imageService) : ControllerBase
    {
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await imageService.DeleteImageAsync(id);
            return Ok(new { Message = "Đã xóa ảnh" });
        }

        [HttpPut("{id}/set-thumbnail")]
        public async Task<IActionResult> SetThumbnail(Guid id)
        {
            await imageService.SetThumbnailAsync(id);
            return Ok(new { Message = "Đã đặt làm ảnh đại diện" });
        }
    }
}
