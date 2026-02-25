namespace NhatSoft.Common.Wrappers;

// Kế thừa ApiResponse<T> -> Tự động có Success, Message, Data...
public class PagedResponse<T> : ApiResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    // Constructor
    public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords, string action = "List Data")
    {
        // 1. Tính toán phân trang
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.TotalRecords = totalRecords;
        this.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        // 2. Gán dữ liệu cho các thuộc tính của LỚP CHA (ApiResponse)
        this.Success = true;
        this.Message = "Thành công";
        this.Description = $"NhatDev - {action}";
        this.Data = data;
        this.Errors = null;
    }
}