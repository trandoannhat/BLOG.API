using System.ComponentModel.DataAnnotations;

namespace NhatSoft.Application.DTOs.Project;

public class CreateProjectDto
{
    // 1. Tên dự án
    [Required(ErrorMessage = "Tên dự án không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên dự án tối đa 200 ký tự")]
    public string Name { get; set; } = string.Empty;

    // 2. Khách hàng
    [MaxLength(100, ErrorMessage = "Tên khách hàng tối đa 100 ký tự")]
    public string ClientName { get; set; } = string.Empty;

    // 3. Mô tả ngắn (Hiện trên Card)
    [MaxLength(500, ErrorMessage = "Mô tả ngắn tối đa 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    // 4. Nội dung chi tiết (HTML/Rich Text) -> Không giới hạn độ dài
    public string Content { get; set; } = string.Empty;

    // 5. Công nghệ
    public List<string> TechStacks { get; set; } = new();

    // 6. Link Demo (Kiểm tra đúng định dạng URL)
    [Url(ErrorMessage = "Link Live Demo không đúng định dạng")]
    public string? LiveDemoUrl { get; set; }

    [Url(ErrorMessage = "Link Source Code không đúng định dạng")]
    public string? SourceCodeUrl { get; set; }

    // 7. Ngày tháng
    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
    public DateTime StartDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public bool IsFeatured { get; set; } = false;

    // --- PHẦN ẢNH (QUAN TRỌNG) ---

    // 8. Thumbnail Url (Ảnh đại diện)
    // Cần thêm cái này để Backend biết ảnh nào là ảnh chính xác định
    public string? ThumbnailUrl { get; set; }

    // 9. Danh sách tất cả ảnh (Bao gồm cả Thumbnail)
    public List<string> ImageUrls { get; set; } = new();
}