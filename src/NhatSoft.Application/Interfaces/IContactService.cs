using NhatSoft.Application.DTOs.Contact;

namespace NhatSoft.Application.Interfaces
{
    public interface IContactService
    {
        Task CreateAsync(CreateContactDto request); // Khách gửi
        Task<(IEnumerable<ContactDto> Data, int TotalRecords)> GetPagedAsync(ContactFilterParams filter); // Admin xem
        Task<ContactDto> GetByIdAsync(Guid id);
        Task UpdateStatusAsync(UpdateContactDto request); // Admin cập nhật
        Task DeleteAsync(Guid id);
    }
}
