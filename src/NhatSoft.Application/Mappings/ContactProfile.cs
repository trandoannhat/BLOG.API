using AutoMapper;
using NhatSoft.Application.DTOs.Contact;
using NhatSoft.Domain.Entities;

namespace NhatSoft.Application.Mappings
{
   public class ContactProfile:Profile
    {
        public ContactProfile()
        {  // ContactProfile.cs
            CreateMap<Contact, ContactDto>();
            CreateMap<CreateContactDto, Contact>();

        }
      
    }
}
