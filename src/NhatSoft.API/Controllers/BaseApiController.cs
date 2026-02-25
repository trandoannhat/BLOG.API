using Microsoft.AspNetCore.Mvc;
using NhatSoft.Common.Wrappers;

namespace NhatSoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // 1. Trả về 200 OK
    protected IActionResult Success<T>(T data, string message = "Thành công", string action = "Operation")
        => Ok(new ApiResponse<T>(data, message, action));

    // 2. Trả về 400 Bad Request
    protected IActionResult Error(string message, string action = "Error")
        => BadRequest(new ApiResponse<string>(message, action));

    // 3. Trả về 200 OK (Phân trang)
    protected IActionResult Paged<T>(T data, int page, int size, int total, string action = "Get List")
        => Ok(new PagedResponse<T>(data, page, size, total, action));

    // --- MỚI THÊM: 4. Trả về 201 Created ---
    // Cách dùng: return CreatedResource(data, "Tạo thành công");
    protected IActionResult CreatedResource<T>(T data, string message = "Tạo mới thành công", string action = "Create")
        => StatusCode(201, new ApiResponse<T>(data, message, action));
}