using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using NhatSoft.Application.DTOs.Account;
using NhatSoft.Application.Interfaces;
using NhatSoft.Application.Settings;
using NhatSoft.Common.Exceptions;
using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Enums; // Dùng để lấy Role mặc định
using NhatSoft.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NhatSoft.Application.Services;

public class AccountService(
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings) : IAccountService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    // 1. ĐĂNG KÝ
    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
        // Check Email đã tồn tại chưa
        var existingUser = (await unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
        if (existingUser != null)
        {
            // Sử dụng cách gán thuộc tính trực tiếp để tránh nhầm lẫn Constructor
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Email này đã được sử dụng."
            };
        }

        // Tạo User Entity mới
        var newUser = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            // Mã hóa mật khẩu
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Client, // Mặc định là User thường (Sẽ chỉnh DB thành Admin sau)
           // IsVerified = true // Tạm thời cho active luôn ( loại bỏ vì entity chưa có)
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.CompleteAsync();

        return new ApiResponse<string>(data: newUser.Id.ToString(), message: "Đăng ký thành công", "register");
    }

    // 2. ĐĂNG NHẬP
    public async Task<ApiResponse<AuthenticationResponse>> AuthenticateAsync(LoginRequest request)
    {
        // Tìm user theo Email
        var user = (await unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();

        // Check user tồn tại và Pass đúng (Verify hash)
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Trả về lỗi chung chung để bảo mật (tránh Hacker đoán được Email đúng)
            return new ApiResponse<AuthenticationResponse>("Thông tin đăng nhập không chính xác.");
        }

        // Tạo Token
        var token = GenerateJwtToken(user);

        // Trả về kết quả
        var response = new AuthenticationResponse
        {
            Id = user.Id.ToString(),
            UserName = user.FullName,
            Email = user.Email,
            Roles = new List<string> { user.Role.ToString() },
            IsVerified = true,
            JWToken = token
        };

        return new ApiResponse<AuthenticationResponse>(response, "Đăng nhập thành công", "Authenticate");
    }

    // --- HÀM RIÊNG: SINH TOKEN JWT ---
    private string GenerateJwtToken(User user)
    {
        // 1. Tạo các Claims (Thông tin đính kèm trong vé)
        var claims = new List<Claim>
        {
            new Claim("id", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // Lưu Role để phân quyền (Admin/User)
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // 2. Lấy Key bí mật từ cấu hình
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Quy định thời hạn vé
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        // 4. Tạo Token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(string userId)
    {
        var user = (await unitOfWork.Users.FindAsync(u => u.Id.ToString() == userId)).FirstOrDefault();
        if (user == null) return new ApiResponse<UserProfileDto>("Không tìm thấy người dùng.");

        var profile = new UserProfileDto
        {
            Id = user.Id.ToString(),
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl
        };
        return new ApiResponse<UserProfileDto>(profile, "Thành công");
    }

    public async Task<ApiResponse<string>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = (await unitOfWork.Users.FindAsync(u => u.Id.ToString() == userId)).FirstOrDefault();

        //  ĐÃ SỬA: Dùng Object Initializer để báo lỗi rõ ràng, dẹp luôn Constructor
        if (user == null)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Không tìm thấy người dùng.",
                Errors = new List<string> { "Không tìm thấy người dùng." },
                Description = "NhatDev - Error"
            };
        }

        // Cập nhật thông tin
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.AvatarUrl = request.AvatarUrl;

        unitOfWork.Users.Update(user);
        await unitOfWork.CompleteAsync();

        // Dùng Object Initializer cho trường hợp thành công
        return new ApiResponse<string>
        {
            Success = true,
            Data = user.Id.ToString(),
            Message = "Cập nhật thành công",
            Description = "NhatDev - UpdateProfile"
        };
    }
}