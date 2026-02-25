// NhatSoft.Common.Wrappers.PaginationFilter.cs
namespace NhatSoft.Common.Wrappers;

public class PaginationFilter
{
    private int _pageNumber;
    private int _pageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value;
    }

    // Từ khóa tìm kiếm chung (Hầu như cái nào cũng cần search text)
    public string? Keyword { get; set; }

    public PaginationFilter()
    {
        this.PageNumber = 1;
        this.PageSize = 10;
    }
}