using NhatSoft.Application.DTOs.Blog;

namespace NhatSoft.Application.Interfaces;

public interface ICategoryService
{
    // 1. Lấy danh sách phân trang (Dùng cho trang Quản lý Admin)
    Task<(IEnumerable<CategoryDto> Data, int TotalRecords)> GetPagedCategoriesAsync(CategoryFilterParams filter);

    // 2. Lấy tất cả (Dùng cho Dropdown chọn danh mục khi viết bài)
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    // 3. Các hàm CRUD cơ bản
    Task<CategoryDto> GetByIdAsync(Guid id);
    Task<CategoryDto> GetBySlugAsync(string slug); // Dùng cho Frontend hiển thị bài theo danh mục
    Task<CategoryDto> CreateAsync(CreateCategoryDto request);
    Task UpdateAsync(UpdateCategoryDto request);
    Task DeleteAsync(Guid id);
}