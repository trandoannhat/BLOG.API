using NhatSoft.Common.Wrappers;

namespace NhatSoft.Application.DTOs.Donation;

public class DonationFilterParams : PaginationFilter
{
    // Keyword đã có sẵn ở lớp cha (PaginationFilter) nên không cần khai báo lại
    // PageNumber, PageSize cũng đã có sẵn ở lớp cha

    // Chỉ thêm các filter đặc thù của Donation
    public bool? IsConfirmed { get; set; }
}