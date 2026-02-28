using NhatSoft.Application.DTOs.Account;
using NhatSoft.Common.Wrappers;

namespace NhatSoft.Application.Interfaces;

public interface IAccountService
{
    // Hàm đăng nhập trả về Token
    Task<ApiResponse<AuthenticationResponse>> AuthenticateAsync(LoginRequest request);

    // Hàm đăng ký user mới
    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);

    Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId);
    Task<ApiResponse<string>> UpdateProfileAsync(string userId, UpdateProfileRequest request);
}
