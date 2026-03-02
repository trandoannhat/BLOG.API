using AutoMapper;
using NhatSoft.Application.DTOs.Project;
using NhatSoft.Common.Helpers; // Để dùng hàm ToSlug
using NhatSoft.Domain.Entities;
using System.Text.Json;

namespace NhatSoft.Application.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        // ==========================================================
        // 1. CREATE: Map từ Request -> Entity
        // ==========================================================
        CreateMap<CreateProjectDto, Project>()
            // Tự động tạo Slug nếu client không truyền lên
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Slug) ? src.Name.ToSlug() : src.Slug))

            // Bỏ qua các trường phức tạp để Service tự xử lý
            .ForMember(dest => dest.TechStackJson, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // ==========================================================
        // 2. UPDATE: Map từ Request -> Entity
        // ==========================================================
        CreateMap<UpdateProjectDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // KHÔNG cho ghi đè ID

            // Cập nhật Slug
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Slug) ? src.Name.ToSlug() : src.Slug))

            // Bỏ qua các trường tự xử lý
            .ForMember(dest => dest.TechStackJson, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // LƯU Ý: Đã gỡ bỏ dest.CreatedAt và dest.CreatedBy để tránh lỗi. 

        // ==========================================================
        // 3. GET: Map từ Entity -> Response DTO
        // ==========================================================
        CreateMap<Project, ProjectResponseDto>()
            // Lấy ảnh đầu tiên làm Thumbnail cực kỳ an toàn (Không sợ NullReference)
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                src.Images != null && src.Images.Any()
                ? (src.Images.FirstOrDefault(x => x.IsThumbnail) ?? src.Images.FirstOrDefault())!.ImageUrl
                : string.Empty))

            // Map tất cả link ảnh ra list string an toàn
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src =>
                src.Images != null ? src.Images.Select(x => x.ImageUrl).ToList() : new List<string>()))

            // Parse JSON an toàn
            .ForMember(dest => dest.TechStacks, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.TechStackJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(src.TechStackJson, (JsonSerializerOptions)null)));

        // ==========================================================
        // 4. MAP ẢNH CON
        // ==========================================================
        CreateMap<ProjectImage, ProjectImageDto>();
    }
}