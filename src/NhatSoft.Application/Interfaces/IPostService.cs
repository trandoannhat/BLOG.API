using NhatSoft.Application.DTOs.Blog;

namespace NhatSoft.Application.Interfaces;

public interface IPostService
{
    Task<(IEnumerable<PostDto> Data, int TotalRecords)> GetPagedPostsAsync(PostFilterParams filter);
    Task<PostDto> GetPostByIdAsync(Guid id);
    Task<PostDto> GetPostBySlugAsync(string slug); // API cho trang chi tiết bài viết
    Task<PostDto> CreatePostAsync(CreatePostDto request, Guid userId); // Cần userId để biết ai viết
    Task UpdatePostAsync(UpdatePostDto request);
    Task DeletePostAsync(Guid id);
}