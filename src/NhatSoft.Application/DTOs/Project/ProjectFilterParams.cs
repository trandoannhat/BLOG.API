// NhatSoft.Application.DTOs.Project.ProjectFilterParams.cs
using NhatSoft.Common.Wrappers;

namespace NhatSoft.Application.DTOs.Project;

//  KẾ THỪA: Nó sẽ tự có PageNumber, PageSize, Keyword
public class ProjectFilterParams : PaginationFilter
{
    // Các thuộc tính riêng của Project
    public bool? IsFeatured { get; set; }   // Lọc dự án nổi bật
    public DateTime? FromDate { get; set; } // Lọc ngày bắt đầu (Từ ngày)
    public DateTime? ToDate { get; set; }   // Lọc ngày bắt đầu (Đến ngày)

    // Sau này muốn lọc theo ClientName riêng biệt thì thêm vào đây
    // public string? ClientName { get; set; } 
}