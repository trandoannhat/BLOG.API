namespace NhatSoft.Application.DTOs.Donation;

public class TopSupporterDto
{
    public string DonorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class DonationStatsDto
{
    public decimal TotalRaised { get; set; }
    public decimal TargetAmount { get; set; } = 2000000; // Mục tiêu: 2.000.000 VNĐ
    public List<TopSupporterDto> TopSupporters { get; set; } = new();
}