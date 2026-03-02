
using NhatSoft.Domain.Entities.Base;

namespace NhatSoft.Domain.Entities;

public class Donation : AuditableEntity
{
    // Tên người ủng hộ (Mặc định ẩn danh nếu họ không nhập)
    public string DonorName { get; set; } = "Ẩn danh";

    // Số tiền ủng hộ
    public decimal Amount { get; set; }

    // Lời nhắn đính kèm
    public string Message { get; set; } = string.Empty;

    // Phương thức (MoMo, ZaloPay, Bank, PayPal...)
    public string PaymentMethod { get; set; } = string.Empty;

    // CỰC KỲ QUAN TRỌNG: Trạng thái duyệt
    // Chỉ những record IsConfirmed = true mới được kéo ra hiển thị ở Frontend
    public bool IsConfirmed { get; set; } = false;
}