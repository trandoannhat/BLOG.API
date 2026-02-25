using NhatSoft.Domain.Entities.Base; 
using NhatSoft.Domain.Interfaces.Base; 

namespace NhatSoft.Domain.Entities;


public class Post : AuditableEntity, ISoftDelete
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;

    public int ViewCount { get; set; } = 0;
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // ---Triển khai ISoftDelete ---
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public void Undo() { IsDeleted = false; DeletedAt = null; }
    // -----------------------------------

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid AuthorId { get; set; }
    public User? Author { get; set; }
}