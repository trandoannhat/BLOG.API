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
        // 1. Khởi tạo Query với các Include cần thiết
        var query = unitOfWork.Posts.GetAllQueryable()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .AsQueryable();

        // --- 2. LOGIC LỌC DANH MỤC (NÂNG CẤP) ---

        // Ưu tiên lọc theo Slug (Dành cho Web Next.js)
        if (!string.IsNullOrEmpty(filter.CategorySlug))
        {
            var category = await unitOfWork.Categories.GetAllQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == filter.CategorySlug);

            if (category != null)
            {
                // Lấy ID của chính nó và tất cả các con trực tiếp của nó
                var categoryIds = await unitOfWork.Categories.GetAllQueryable()
                    .Where(c => c.Id == category.Id || c.ParentId == category.Id)
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }
            else
            {
                // Nếu slug sai, trả về rỗng luôn để tránh hiện bài viết lung tung
                return (new List<PostDto>(), 0);
            }
        }
        // Nếu không có Slug thì lọc theo ID (Dành cho trang Admin)
        else if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        // --- 3. CÁC FILTER KHÁC ---
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(p => p.Title.ToLower().Contains(keyword) ||
                                     p.Summary.ToLower().Contains(keyword));
        }

        if (filter.AuthorId.HasValue)
            query = query.Where(p => p.AuthorId == filter.AuthorId.Value);

        if (filter.IsPublished.HasValue)
            query = query.Where(p => p.IsPublished == filter.IsPublished.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);

        // --- 4. SORT & PAGING ---
        var totalRecords = await query.CountAsync();

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (mapper.Map<IEnumerable<PostDto>>(posts), totalRecords);
    }

    // 2. CREATE
    // 2. CREATE
    public async Task<PostDto> CreatePostAsync(CreatePostDto request, Guid authorId)
    {
        // Check Category tồn tại
        var category = await unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null) throw new NotFoundException("Danh mục không tồn tại");

        var post = mapper.Map<Post>(request);
        post.AuthorId = authorId;

        // --- TẠO SLUG VÀ XỬ LÝ TRÙNG LẶP ---
        string baseSlug = post.Title.ToSlug(); // Gọi Helper lọc từ nối
        string finalSlug = baseSlug;
        int counter = 1;

        // Vòng lặp check trùng Slug trong Database
        while (await unitOfWork.Posts.GetAllQueryable().AnyAsync(p => p.Slug == finalSlug))
        {
            finalSlug = $"{baseSlug}-{counter}";
            counter++;
        }
        post.Slug = finalSlug;

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
    // 3. UPDATE
    public async Task UpdatePostAsync(UpdatePostDto request)
    {
        var post = await unitOfWork.Posts.GetByIdAsync(request.Id);
        if (post == null) throw new NotFoundException("Bài viết không tồn tại");

        // Map dữ liệu mới vào entity cũ
        mapper.Map(request, post);

        // --- CẬP NHẬT LẠI SLUG KHI LƯU ---
        // (Bạn mở bài cũ lên bấm Lưu là nó tự chạy qua bộ lọc mới)
        string baseSlug = post.Title.ToSlug();
        string finalSlug = baseSlug;
        int counter = 1;

        // Check trùng slug (Lưu ý: Phải trừ chính ID của bài viết hiện tại ra)
        while (await unitOfWork.Posts.GetAllQueryable()
               .AnyAsync(p => p.Slug == finalSlug && p.Id != post.Id))
        {
            finalSlug = $"{baseSlug}-{counter}";
            counter++;
        }
        post.Slug = finalSlug;

        // Logic ngày đăng: Nếu trước đó chưa đăng, giờ mới đăng -> Set ngày
        if (post.IsPublished && post.PublishedAt == null)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        // Nếu un-publish -> Xóa ngày đăng
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
            .Where(p => p.IsPublished == true) // --- THÊM DÒNG NÀY để loại bản nháp---
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