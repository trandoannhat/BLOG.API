using NhatSoft.Domain.Entities.Base; 
using NhatSoft.Domain.Enums;
using NhatSoft.Domain.Interfaces.Base;

namespace NhatSoft.Domain.Entities;

public class User : AuditableEntity, ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Client;

    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // ==========================================
    //  TRIỂN KHAI 3 THÀNH PHẦN CỦA ISOFTDELETE
    // ==========================================
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Logic khi muốn khôi phục lại tài khoản đã xóa
    public void Undo()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}