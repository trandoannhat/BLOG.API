namespace NhatSoft.Domain.Enums;

public enum ContactStatus
{
    New = 0,        // Mới nhận
    Processing = 1, // Đang tư vấn
    Replied = 2,    // Đã trả lời xong
    Ignored = 3     // Spam/Bỏ qua
}
