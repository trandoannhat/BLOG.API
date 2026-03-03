using NhatSoft.Domain.Entities.Base;

namespace NhatSoft.Domain.Entities;

public class SystemSetting: BaseEntity
{
    //public Guid Id { get; set; }
    public string Key { get; set; } = null!; // Ví dụ: "DonationTarget"
    public string Value { get; set; } = null!; // Ví dụ: "1000000"
    public string? Description { get; set; } // Ví dụ: "Mục tiêu donate duy trì server"
}
