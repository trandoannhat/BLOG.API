namespace NhatSoft.Application.DTOs.Donation;

public class DonationResponseDto
{
    public Guid Id { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }


}
