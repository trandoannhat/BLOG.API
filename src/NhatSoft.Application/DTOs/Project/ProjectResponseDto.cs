namespace NhatSoft.Application.DTOs.Project;

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> TechStacks { get; set; } = new();
    public string? LiveDemoUrl { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty; // Ảnh đại diện
    public List<string> ImageUrls { get; set; } = new(); // Tất cả ảnh
}