using NhatSoft.Domain.Entities.Base;
using NhatSoft.Domain.Interfaces.Base;

namespace NhatSoft.Domain.Entities;
// Kế thừa AuditableEntity và Implement ISoftDelete
public class Project : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL thân thiện SEO
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Bài viết chi tiết về Case Study này

    // Công nghệ sử dụng (Lưu JSON: ["React", ".NET 10", "PostgreSQL"])
    public string TechStackJson { get; set; } = "[]";

    public string? LiveDemoUrl { get; set; }
    public string? SourceCodeUrl { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public bool IsFeatured { get; set; } = false; // Đưa lên trang chủ?

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public void Undo() { IsDeleted = false; DeletedAt = null; }

    // Quan hệ: 1 Project có nhiều ảnh
    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
}

