using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.PartnerAd;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Constants;
using NhatSoft.Domain.Enums;

namespace NhatSoft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerAdsController(IPartnerAdService partnerAdService) : ControllerBase
    {
        [HttpGet("public/{position}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveAds(AdPosition position)
        {
            var data = await partnerAdService.GetActiveAdsByPositionAsync(position);
            return Ok(data);
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var data = await partnerAdService.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await partnerAdService.GetByIdAsync(id);
            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] CreatePartnerAdDto request)
        {
            var data = await partnerAdService.CreateAsync(request);
            // Có thể wrap bằng ApiResponse tại đây nếu Frontend của bạn cần
            return Ok(data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerAdDto request)
        {
            if (id != request.Id) return BadRequest("ID không khớp");
            await partnerAdService.UpdateAsync(request);
            return Ok(new { Message = "Đã cập nhật quảng cáo" });
        }

        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await partnerAdService.ToggleActiveAsync(id);
            return Ok(new { Message = "Đã đổi trạng thái quảng cáo" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await partnerAdService.DeleteAsync(id);
            return Ok(new { Message = "Đã xóa quảng cáo" });
        }
    }
}