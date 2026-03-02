using System.ComponentModel.DataAnnotations;

namespace NhatSoft.Application.DTOs.Donation;

public class CreateDonationDto
{
    [MaxLength(100)]
    public string DonorName { get; set; } = "Ẩn danh";

    [Required(ErrorMessage = "Vui lòng nhập số tiền")]
    [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu là 1,000đ")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn phương thức")]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;



}
