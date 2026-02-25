using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Blog;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ICategoryService categoryService) : ControllerBase
    {
        // 1. GET ALL (Dropdown)
        // API: GET /api/categories/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllForDropdown()
        {
            var result = await categoryService.GetAllAsync();
            return Ok(new { Data = result });
        }

        // 2. GET PAGED (Table Admin)
        // API: GET /api/categories?PageNumber=1&Keyword=...
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] CategoryFilterParams filter)
        {
            var (data, total) = await categoryService.GetPagedCategoriesAsync(filter);
            return Ok(new
            {
                Data = data,
                TotalRecords = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }

        // 3. GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await categoryService.GetByIdAsync(id);
            return Ok(new { Data = result });
        }

        // 4. CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto request)
        {
            var result = await categoryService.CreateAsync(request);
            return Ok(new { Message = "Tạo danh mục thành công", Data = result });
        }

        // 5. UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateCategoryDto request)
        {
            if (id != request.Id) return BadRequest("ID không khớp");
            await categoryService.UpdateAsync(request);
            return Ok(new { Message = "Cập nhật thành công" });
        }

        // 6. DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await categoryService.DeleteAsync(id);
            return Ok(new { Message = "Đã xóa danh mục" });
        }
    }
}
