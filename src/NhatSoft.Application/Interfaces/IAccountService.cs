using NhatSoft.Application.DTOs.Account;
using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Enums;

namespace NhatSoft.Application.Interfaces;

public interface IAccountService
{
    /// <summary>
    /// Xử lý đăng nhập, xác thực thông tin và trả về JWT Token.
    /// </summary>
    /// <param name="request">Thông tin đăng nhập (Email, Password)</param>
    /// <returns>Dữ liệu user kèm JWT Token</returns>
    Task<ApiResponse<AuthenticationResponse>> AuthenticateAsync(LoginRequest request);

    /// <summary>
    /// Đăng ký tài khoản người dùng mới vào hệ thống.
    /// Mặc định gán Role là Khách hàng (Client/User).
    /// </summary>
    /// <param name="request">Thông tin đăng ký</param>
    /// <returns>ID của User vừa tạo</returns>
    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Lấy thông tin hồ sơ cá nhân của người dùng dựa vào ID trong Token.
    /// </summary>
    /// <param name="userId">Mã định danh của người dùng</param>
    /// <returns>Chi tiết hồ sơ người dùng</returns>
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId);

    /// <summary>
    /// Cập nhật thông tin hồ sơ cá nhân (Tên, Số điện thoại, Avatar...).
    /// </summary>
    /// <param name="userId">Mã định danh của người dùng</param>
    /// <param name="request">Dữ liệu cần cập nhật</param>
    /// <returns>Thông báo cập nhật thành công</returns>
    Task<ApiResponse<string>> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    // ==========================================
    // ADMIN ENDPOINTS
    // ==========================================

    /// <summary>
    /// Lấy danh sách toàn bộ người dùng trong hệ thống (Dành cho Admin).
    /// </summary>
    Task<ApiResponse<IEnumerable<UserProfileDto>>> GetAllUsersAsync();

    


    /// <summary>
    /// Cập nhật quyền (Role) cho người dùng (Dành cho Admin).
    /// </summary>
    /// <param name="currentUserId">Mã định danh của Admin đang thực hiện thao tác</param>
    /// <param name="targetUserId">Mã định danh người dùng bị thay đổi</param>
    /// <param name="newRole">Quyền mới (Enum)</param>
    Task<ApiResponse<string>> UpdateUserRoleAsync(string currentUserId, string targetUserId, UserRole newRole);


    /// <summary>
    /// Xóa tài khoản người dùng khỏi hệ thống (Dành cho Admin).
    /// </summary>
    /// <param name="currentUserId">Mã định danh của Admin đang thao tác</param>
    /// <param name="targetUserId">Mã định danh người dùng bị xóa</param>
    Task<ApiResponse<string>> DeleteUserAsync(string currentUserId, string targetUserId);
}