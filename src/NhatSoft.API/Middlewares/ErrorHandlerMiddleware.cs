using NhatSoft.Common.Exceptions;
using NhatSoft.Common.Wrappers;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NhatSoft.API.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            await HandleExceptionAsync(context, error);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception error)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        // Khởi tạo model trả về mặc định (Lỗi)
        // Constructor này đã tự set Success = false và Description = "NhatDev - Error"
        var responseModel = new ApiResponse<string>(error.Message, "Error");

        switch (error)
        {
            case Common.Exceptions.ValidationException e:
                // Lỗi 400: Dữ liệu input sai
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseModel.Errors = e.Errors;
                responseModel.Message = "Dữ liệu không hợp lệ";
                responseModel.Description = "NhatDev - Validation Error";
                break;

            case ApiException e:
                // Lỗi Logic chung (ví dụ: Email đã tồn tại)
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case NotFoundException e:
                // Lỗi 404: Không tìm thấy
                response.StatusCode = (int)HttpStatusCode.NotFound;
                responseModel.Description = "NhatDev - Not Found";
                break;

            case KeyNotFoundException e:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            default:
                // Lỗi 500: Server chết (Lỗi code, lỗi DB...)
                _logger.LogError(error, error.Message); // Ghi log để dev sửa

                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                responseModel.Message = "Lỗi hệ thống (Internal Server Error)";
                // Với lỗi 500, không nên show chi tiết lỗi cho user (bảo mật), trừ khi đang Dev
                // responseModel.Errors = new List<string> { error.Message }; 
                break;
        }

        var result = JsonSerializer.Serialize(responseModel, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}