namespace NhatSoft.Common.Wrappers;

public class PaginationFilter
{
    private int _pageNumber;
    private int _pageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value; // Chặn ngay khi gán giá trị
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value; // Chặn ngay khi gán giá trị
    }

    public PaginationFilter()
    {
        // Mặc định
        this.PageNumber = 1;
        this.PageSize = 10;
    }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        // Gọi qua property để kích hoạt logic ở trên
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
    }
}