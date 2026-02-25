using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NhatSoft.Application.DTOs.Contact;

// 1. CREATE (Public - Khách gửi)
public class CreateContactDto
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    [Required] public string Subject { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;
}

// 2. RESPONSE (Admin xem)
public class ContactDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public ContactStatus Status { get; set; }
    public string? AdminNote { get; set; } // Ghi chú của Admin
    public DateTime CreatedAt { get; set; }
}

// 3. UPDATE (Admin xử lý)
public class UpdateContactDto
{
    public Guid Id { get; set; }
    public ContactStatus Status { get; set; }
    public string? AdminNote { get; set; }
}

// 4. FILTER
public class ContactFilterParams : PaginationFilter
{
    public ContactStatus? Status { get; set; } // Lọc theo trạng thái
}