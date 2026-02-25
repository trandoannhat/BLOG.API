using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Blog;
using NhatSoft.Application.Interfaces;
using System.Security.Claims;

namespace NhatSoft.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // 🚨 KHÓA TOÀN BỘ CONTROLLER: Bắt buộc đăng nhập với mọi thao tác
public class PostsController(IPostService postService) : ControllerBase
{
    // ==========================================
    // KHU VỰC PUBLIC (Khách vãng lai được xem)
    // ==========================================

    [HttpGet]
    [AllowAnonymous] // ✅ Mở khóa cho phép Web Next.js lấy danh sách
    public async Task<IActionResult> GetAll([FromQuery] PostFilterParams filter)
    {
        var (data, total) = await postService.GetPagedPostsAsync(filter);
        return Ok(new { Data = data, TotalRecords = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize });
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // ✅ Mở khóa
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await postService.GetPostByIdAsync(id);
        return Ok(new { Data = post });
    }

    [HttpGet("byslug/{slug}")]
    [AllowAnonymous] // ✅ Mở khóa
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await postService.GetPostBySlugAsync(slug);
        return Ok(new { Data = post });
    }

    // ==========================================
    // KHU VỰC ADMIN (Bắt buộc phải có JWT Token)
    // ==========================================

    [HttpPost]
    // Không cần ghi [Authorize] nữa vì đã kế thừa từ Class
    public async Task<IActionResult> Create(CreatePostDto request)
    {
        var userIdString = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized(new { Message = "Không thể xác thực thông tin tài khoản." });
        }

        var result = await postService.CreatePostAsync(request, userId);

        return Ok(new { Success = true, Message = "Tạo bài viết thành công", Data = result });
    }

    [HttpPut("{id}")]
    // Đã được bảo vệ an toàn
    public async Task<IActionResult> Update(Guid id, UpdatePostDto request)
    {
        if (id != request.Id) return BadRequest("ID không khớp");
        await postService.UpdatePostAsync(request);
        return Ok(new { Message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    // Đã được bảo vệ an toàn
    public async Task<IActionResult> Delete(Guid id)
    {
        await postService.DeletePostAsync(id);
        return Ok(new { Message = "Đã xóa bài viết" });
    }
}