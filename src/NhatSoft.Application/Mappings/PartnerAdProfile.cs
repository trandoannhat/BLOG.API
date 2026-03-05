using AutoMapper;
using NhatSoft.Application.DTOs.PartnerAd;
using NhatSoft.Domain.Entities;

namespace NhatSoft.Application.Mappings;

public class PartnerAdProfile : Profile
{
    public PartnerAdProfile()
    {
        // Từ Entity -> DTO (để trả về dữ liệu)
        CreateMap<PartnerAd, PartnerAdDto>();

        // Từ DTO -> Entity (để Thêm mới / Cập nhật)
        CreateMap<CreatePartnerAdDto, PartnerAd>();
        CreateMap<UpdatePartnerAdDto, PartnerAd>();
    }
}