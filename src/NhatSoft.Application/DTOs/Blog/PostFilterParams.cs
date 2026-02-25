using NhatSoft.Common.Wrappers;

namespace NhatSoft.Application.DTOs.Blog;

public class PostFilterParams : PaginationFilter
{
    public Guid? CategoryId { get; set; } // Lọc theo danh mục
    public Guid? AuthorId { get; set; }   // Lọc bài của tác giả nào
    public bool? IsPublished { get; set; } // Lọc: true=Đã đăng, false=Nháp, null=Tất cả
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}