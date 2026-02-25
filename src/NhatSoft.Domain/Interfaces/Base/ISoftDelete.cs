namespace NhatSoft.Domain.Interfaces.Base;
// 2. Chỉ những thằng nào cần Xóa Mềm mới dùng cái này
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; } // Nên có thêm ngày xóa để tracking
    void Undo(); // Hàm khôi phục (nếu cần logic domain)
}


