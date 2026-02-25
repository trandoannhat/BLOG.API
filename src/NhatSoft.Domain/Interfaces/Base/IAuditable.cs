namespace NhatSoft.Domain.Interfaces.Base;

// 1. Chỉ những thằng nào cần biết ngày giờ tạo/sửa mới dùng cái này
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
