using NhatSoft.Domain.Interfaces.Base;

namespace NhatSoft.Domain.Entities.Base; 

public abstract class AuditableEntity : BaseEntity, IAuditable
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}