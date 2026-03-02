using AutoMapper;
using NhatSoft.Application.DTOs.Donation;
using NhatSoft.Domain.Entities;

namespace NhatSoft.Application.Mappings;


public class DonationProfile : Profile
{
    public DonationProfile()
    {
        CreateMap<CreateDonationDto, Donation>();
        CreateMap<Donation, DonationResponseDto>();
    }
}
