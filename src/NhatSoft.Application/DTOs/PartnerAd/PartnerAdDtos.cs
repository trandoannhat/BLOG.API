using NhatSoft.Domain.Enums;

namespace NhatSoft.Application.DTOs.PartnerAd;

public class PartnerAdDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string TargetUrl { get; set; }
    public AdPosition Position { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePartnerAdDto
{
    public string Title { get; set; }
    public string? ImageUrl { get; set; }
    public string TargetUrl { get; set; }
    public AdPosition Position { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePartnerAdDto : CreatePartnerAdDto
{
    public Guid Id { get; set; }
}
