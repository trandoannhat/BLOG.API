using AutoMapper;
using Microsoft.EntityFrameworkCore; // Để dùng .Include()
using NhatSoft.Application.DTOs.Blog;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;
using NhatSoft.Common.Helpers;
namespace NhatSoft.Application.Services;

public class PostService(IUnitOfWork unitOfWork, IMapper mapper) : IPostService
{
    // 1. GET PAGED LIST (Admin & User)
    public async Task<(IEnumerable<PostDto> Data, int TotalRecords)> GetPagedPostsAsync(PostFilterParams filter)
    {
        // Phải Include Category và Author để map tên ra DTO
        var query = unitOfWork.Posts.GetAllQueryable()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .AsQueryable();

        // --- FILTER ---
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(p => p.Title.ToLower().Contains(keyword) || p.Summary.Contains(keyword));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.IsPublished.HasValue)
            query = query.Where(p => p.IsPublished == filter.IsPublished.Value);

        // Nếu là user thường xem blog -> Chỉ hiện bài đã Published (Logic này có thể tách ra service riêng hoặc filter ở Controller)
        // query = query.Where(p => p.IsPublished == true); 

        // --- SORT & PAGING ---
        var totalRecords = await query.CountAsync();

        var posts = await query
            .OrderByDescending(p => p.CreatedAt) // Mới nhất lên đầu
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (mapper.Map<IEnumerable<PostDto>>(posts), totalRecords);
    }

    // 2. CREATE
    public async Task<PostDto> CreatePostAsync(CreatePostDto request, Guid authorId)
    {
        // Check Category tồn tại
        var category = await unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null) throw new NotFoundException("Danh mục không tồn tại");

        var post = mapper.Map<Post>(request);

        // --- XỬ LÝ LOGIC ---
        post.AuthorId = authorId;

        // Tạo Slug (Dùng thư viện hoặc Helper)
        post.Slug = post.Title.ToSlug(); ;//SlugHelper.GenerateSlug(post.Title);
        // TODO: Check trùng slug trong DB, nếu trùng thì thêm -1, -2...

        // Xử lý ngày đăng
        if (post.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        await unitOfWork.Posts.AddAsync(post);
        await unitOfWork.CompleteAsync();

        return mapper.Map<PostDto>(post);
    }

    // 3. UPDATE
    public async Task UpdatePostAsync(UpdatePostDto request)
    {
        var post = await unitOfWork.Posts.GetByIdAsync(request.Id);
        if (post == null) throw new NotFoundException("Bài viết không tồn tại");

        // Map dữ liệu mới vào entity cũ
        mapper.Map(request, post);

        // Cập nhật Slug nếu Title đổi (Tùy nghiệp vụ, thường thì hạn chế đổi slug vì hỏng SEO)
        // post.Slug = SlugHelper.GenerateSlug(post.Title);

        // Logic ngày đăng: Nếu trước đó chưa đăng, giờ mới đăng -> Set ngày
        if (post.IsPublished && post.PublishedAt == null)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        // Nếu un-publish -> Xóa ngày đăng? Tùy nghiệp vụ
        if (!post.IsPublished)
        {
            post.PublishedAt = null;
        }

        unitOfWork.Posts.Update(post);
        await unitOfWork.CompleteAsync();
    }

    // ... Các hàm GetById, Delete tương tự ProjectService
    public async Task DeletePostAsync(Guid id)
    {
        var post = await unitOfWork.Posts.GetByIdAsync(id);
        if (post == null) throw new NotFoundException("Bài viết không tồn tại");
        unitOfWork.Posts.Delete(post); // Soft Delete do Repository xử lý
        await unitOfWork.CompleteAsync();
    }

    public async Task<PostDto> GetPostByIdAsync(Guid id)
    {
        // Nhớ Include Category
        // Cách tốt nhất là viết hàm GetByIdWithDetailsAsync trong Repo
        var post = await unitOfWork.Posts.GetByIdAsync(id);
        if (post == null) throw new NotFoundException("Not Found");
        return mapper.Map<PostDto>(post);
    }

    public async Task<PostDto> GetPostBySlugAsync(string slug)
    {
        // 1. Dùng GetQueryable để có thể Include Category và Author (rất quan trọng để hiển thị tên)
        var post = await unitOfWork.Posts.GetAllQueryable()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        // 2. Kiểm tra tồn tại
        if (post == null)
        {
            throw new NotFoundException("Bài viết không tồn tại");
        }

        // 3. Tự động tăng lượt xem thêm 1 mỗi khi gọi hàm này
        post.ViewCount += 1;
        unitOfWork.Posts.Update(post);
        await unitOfWork.CompleteAsync();

        // 4. Map ra DTO và trả về
        return mapper.Map<PostDto>(post);
    }
}