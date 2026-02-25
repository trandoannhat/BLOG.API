using NhatSoft.Domain.Entities.Base;

namespace NhatSoft.Domain.Entities;
// Kế thừa BaseEntity thôi, tự thêm CreatedAt nếu muốn
public class ProjectImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; } // Chú thích ảnh
    public bool IsThumbnail { get; set; } = false; // Ảnh đại diện danh sách
    public int SortOrder { get; set; } = 0; // Thứ tự hiển thị
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Chỉ cần ngày tạo
    // Không cần UpdatedAt, Không cần IsDeleted
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
}
