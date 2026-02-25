namespace NhatSoft.Application.DTOs.Project;

public class ProjectImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public string? Caption { get; set; }
    public bool IsThumbnail { get; set; }
    public int SortOrder { get; set; }
}