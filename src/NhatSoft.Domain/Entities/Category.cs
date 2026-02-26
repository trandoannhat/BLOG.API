using NhatSoft.Domain.Entities.Base;
using NhatSoft.Domain.Interfaces.Base;

namespace NhatSoft.Domain.Entities;

public class Category : AuditableEntity 
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Quan hệ: 1 Category có nhiều Post
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // Cột mới thêm vào
    public Guid? ParentId { get; set; }

    // Navigation properties cho EF Core
    public virtual Category ParentCategory { get; set; }
    public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
}
