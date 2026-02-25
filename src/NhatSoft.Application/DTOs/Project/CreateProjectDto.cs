using System.ComponentModel.DataAnnotations;

namespace NhatSoft.Application.DTOs.Project;

public class CreateProjectDto
{
    [Required(ErrorMessage = "Tên dự án không được để trống")]
    public string Name { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Nhận vào danh sách tên công nghệ (VD: ["React", "NetCore"])
    public List<string> TechStacks { get; set; } = new();

    public string? LiveDemoUrl { get; set; }
    public string? SourceCodeUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsFeatured { get; set; }

    // Danh sách URL ảnh (Frontend sẽ upload ảnh trước rồi gửi link vào đây)
    public List<string> ImageUrls { get; set; } = new();
}