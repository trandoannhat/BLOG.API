using AutoMapper;
using NhatSoft.Application.DTOs.Project;
using NhatSoft.Common.Helpers; // Để dùng hàm ToSlug
using NhatSoft.Domain.Entities;
using System.Text.Json;

namespace NhatSoft.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 1. CREATE: Map từ Request -> Entity
        CreateMap<CreateProjectDto, Project>()
            // Tự động tạo Slug từ Name
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Name.ToSlug()))
            // Bỏ qua TechStackJson để Service tự xử lý (hoặc viết Converter phức tạp ở đây)
            .ForMember(dest => dest.TechStackJson, opt => opt.Ignore())
            // Bỏ qua Images để Service tự xử lý vòng lặp
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // 2. GET: Map từ Entity -> Response DTO
        CreateMap<Project, ProjectResponseDto>()
            // Lấy ảnh đầu tiên làm Thumbnail
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(x => x.IsThumbnail).ImageUrl ?? src.Images.FirstOrDefault().ImageUrl))
            // Map tất cả link ảnh ra list string
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.Select(x => x.ImageUrl).ToList()))
            // Parse JSON lại thành List String
            .ForMember(dest => dest.TechStacks, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.TechStackJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(src.TechStackJson, (JsonSerializerOptions)null)));
        // 2. Update: Map từ Request -> Entity
        CreateMap<UpdateProjectDto, Project>()
        .ForMember(dest => dest.Id, opt => opt.Ignore()) // Không cho sửa ID
        .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Name.ToSlug())) // Cập nhật lại Slug nếu tên đổi
        .ForMember(dest => dest.TechStackJson, opt => opt.Ignore()) // Tự xử lý JSON
        .ForMember(dest => dest.Images, opt => opt.Ignore()); // Tự xử lý Ảnh
    }
}