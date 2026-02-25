namespace NhatSoft.Application.DTOs.Blog;

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty; // Sapo / Mô tả ngắn
    public string Content { get; set; } = string.Empty; // HTML
    public string ThumbnailUrl { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = false;
    public Guid CategoryId { get; set; }
}

public class UpdatePostDto : CreatePostDto
{
    public Guid Id { get; set; }
}