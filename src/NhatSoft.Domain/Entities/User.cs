using NhatSoft.Domain.Entities.Base; 
using NhatSoft.Domain.Enums;

namespace NhatSoft.Domain.Entities;

public class User : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Admin;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}