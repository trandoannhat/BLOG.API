using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.DTOs.Contact;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

public class ContactService(IUnitOfWork unitOfWork, IMapper mapper) : IContactService
{
    public async Task CreateAsync(CreateContactDto request)
    {
        var contact = mapper.Map<Contact>(request);
        await unitOfWork.Contacts.AddAsync(contact);
        await unitOfWork.CompleteAsync();
    }

    public async Task<(IEnumerable<ContactDto> Data, int TotalRecords)> GetPagedAsync(ContactFilterParams filter)
    {
        var query = unitOfWork.Contacts.GetAllQueryable();

        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            var key = filter.Keyword.ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(key) || x.Email.ToLower().Contains(key));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (mapper.Map<IEnumerable<ContactDto>>(items), total);
    }

    public async Task UpdateStatusAsync(UpdateContactDto request)
    {
        var contact = await unitOfWork.Contacts.GetByIdAsync(request.Id);
        if (contact == null) throw new NotFoundException("Không tìm thấy liên hệ");

        contact.Status = request.Status;
        contact.AdminNote = request.AdminNote;

        unitOfWork.Contacts.Update(contact);
        await unitOfWork.CompleteAsync();
    }

    // GetById và Delete tương tự...
    public async Task<ContactDto> GetByIdAsync(Guid id)
    {
        var contact = await unitOfWork.Contacts.GetByIdAsync(id);
        if (contact == null) throw new NotFoundException("Not Found");
        return mapper.Map<ContactDto>(contact);
    }

    public async Task DeleteAsync(Guid id)
    {
        var contact = await unitOfWork.Contacts.GetByIdAsync(id);
        if (contact == null) throw new NotFoundException("Not Found");
        unitOfWork.Contacts.Delete(contact);
        await unitOfWork.CompleteAsync();
    }
}