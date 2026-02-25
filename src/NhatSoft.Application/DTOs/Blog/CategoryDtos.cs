using System.ComponentModel.DataAnnotations;
using NhatSoft.Common.Wrappers;

namespace NhatSoft.Application.DTOs.Blog;

// ==========================================
// 1. RESPONSE DTO (Hiển thị ra FE)
// ==========================================
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Thêm trường này: Đếm số bài viết trong danh mục (Rất cần cho Sidebar blog)
    public int PostCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ==========================================
// 2. CREATE REQUEST (Tạo mới)
// ==========================================
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Slug sẽ tự sinh từ Name -> Không cần gửi lên
}

// ==========================================
// 3. UPDATE REQUEST (Cập nhật)
// ==========================================
public class UpdateCategoryDto : CreateCategoryDto
{
    [Required]
    public Guid Id { get; set; }
}

// ==========================================
// 4. FILTER (Bộ lọc tìm kiếm)
// ==========================================
public class CategoryFilterParams : PaginationFilter
{
    // Category thường đơn giản, chỉ cần tìm theo Keyword (Tên) là đủ
    // Nếu sau này cần lọc ẩn/hiện thì thêm:
    // public bool? IsVisible { get; set; }
}