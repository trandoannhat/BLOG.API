using AutoMapper;
using NhatSoft.Application.DTOs.Blog;
using NhatSoft.Common.Helpers; // Để dùng hàm .ToSlug()
using NhatSoft.Domain.Entities;

namespace NhatSoft.Application.Mappings;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        // ======================================================
        // 1. CATEGORY MAPPING
        // ======================================================

        // Entity -> DTO (Hiển thị)
        CreateMap<Category, CategoryDto>()
            // Map số lượng bài viết (PostCount)
            .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.Posts.Count));

        // Create -> Entity
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // Tự động tạo Slug từ Name
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Name.ToSlug()));

        // Update -> Entity
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Không cho sửa ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            // Cập nhật lại Slug nếu đổi tên
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Name.ToSlug()));

        // ======================================================
        // 2. POST MAPPING
        // ======================================================

        // Entity -> Response DTO (Hiển thị)
        CreateMap<Post, PostDto>()
            // Làm phẳng (Flatten) dữ liệu: Lấy tên Category thay vì trả về cả object
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"))

            // Lấy tên Tác giả
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : "Ẩn danh")) // Giả sử User có FullName

            // Format ngày tháng nếu cần (thường thì để FE format, BE trả về chuẩn ISO)
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.PublishedAt));

        // Create Request -> Entity
        CreateMap<CreatePostDto, Post>()
            // Tự động Slug
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Title.ToSlug()))

            // Mặc định ViewCount = 0 khi tạo mới
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())

            // Xử lý ngày đăng: Nếu IsPublished = true thì lấy giờ hiện tại (Hoặc xử lý trong Service sẽ an toàn hơn)
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.IsPublished ? DateTime.UtcNow : (DateTime?)null));

        // Update Request -> Entity
        CreateMap<UpdatePostDto, Post>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Không sửa ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Không sửa ngày tạo
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore()) // Không reset view
            .ForMember(dest => dest.AuthorId, opt => opt.Ignore()) // Không đổi tác giả

            // Nếu đổi tiêu đề -> Cập nhật Slug
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Title.ToSlug()));
    }
}