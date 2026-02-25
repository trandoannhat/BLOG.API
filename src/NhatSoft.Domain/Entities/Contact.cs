using NhatSoft.Domain.Entities.Base; 
using NhatSoft.Domain.Enums;

namespace NhatSoft.Domain.Entities;

public class Contact : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ContactStatus Status { get; set; } = ContactStatus.New;
    public string? AdminNote { get; set; }
}