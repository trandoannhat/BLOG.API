namespace NhatSoft.Application.DTOs.Blog;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }
    public string ThumbnailUrl { get; set; }

    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } // Lấy từ AuditableEntity

    // Thông tin Category (Flatten ra cho dễ dùng ở FE)
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }

    // Thông tin Tác giả
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
}