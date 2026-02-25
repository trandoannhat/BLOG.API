namespace NhatSoft.Common.Wrappers;

public class ApiResponse<T>
{
    
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Description { get; set; }
    public List<string>? Errors { get; set; }
    public T? Data { get; set; }

    public ApiResponse() { }

    // --- Constructor THÀNH CÔNG ---
    public ApiResponse(T data, string message = "Thành công", string action = "Operation")
    {
        Success = true; // Set Success = true
        Message = message;
        Description = $"NhatDev - {action}";
        Data = data;
        Errors = null;
    }

    // --- Constructor THẤT BẠI ---
    public ApiResponse(string message, string action = "Error")
    {
        Success = false; // Set Success = false
        Message = message;
        Description = $"NhatDev - {action}";
        Errors = new List<string> { message };
    }

    // --- Constructor VALIDATE LỖI ---
    public ApiResponse(List<string> errors, string action = "Validation")
    {
        Success = false;
        Message = "Dữ liệu không hợp lệ";
        Description = $"NhatDev - {action}";
        Errors = errors;
    }
}
