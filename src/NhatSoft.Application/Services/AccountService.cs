

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using NhatSoft.Application.DTOs.Account;
using NhatSoft.Application.Interfaces;
using NhatSoft.Application.Settings;
using NhatSoft.Common.Exceptions;
using NhatSoft.Common.Wrappers;
// 👇 IMPORT AppConstants
using NhatSoft.Common.Constants;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Enums;
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
        var existingUser = (await unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
        if (existingUser != null)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Email này đã được sử dụng."
            };
        }

        var newUser = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

            // 👇 SỬA LẠI: Gán Role mặc định bằng hằng số từ AppConstants (Nếu Entity lưu kiểu String)
            // Lưu ý: Nếu Entity User của bạn đang định nghĩa public UserRole Role {get;set;} 
            // thì bạn VẪN giữ nguyên UserRole.Client ở đây, và chỉ map sang string ở phần Token bên dưới nhé.
            // Giả định ở đây Entity lưu String:
            //Role = AppConstants.Roles.User
            //  FIX Ở ĐÂY: Trả về kiểu Enum vì DB của bạn đang dùng Enum
            Role = UserRole.Client
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.CompleteAsync();

        return new ApiResponse<string>(data: newUser.Id.ToString(), message: "Đăng ký thành công", "register");
    }

    // 2. ĐĂNG NHẬP
    public async Task<ApiResponse<AuthenticationResponse>> AuthenticateAsync(LoginRequest request)
    {
        var user = (await unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new ApiResponse<AuthenticationResponse>("Thông tin đăng nhập không chính xác.");
        }

        var token = GenerateJwtToken(user);

        // 👇 Trả về Roles để bên Frontend (React/Zustand) hứng được chữ "Admin"
        var response = new AuthenticationResponse
        {
            Id = user.Id.ToString(),
            UserName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            // Chắc chắn rẳng chuỗi này trùng với chuỗi đã lưu
            Roles = new List<string> { user.Role.ToString() },
            IsVerified = true,
            JWToken = token
        };

        return new ApiResponse<AuthenticationResponse>(response, "Đăng nhập thành công", "Authenticate");
    }

    // --- HÀM RIÊNG: SINH TOKEN JWT ---
    private string GenerateJwtToken(User user)
    {
        // 1. Tạo các Claims
        var claims = new List<Claim>
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Bồi thêm cái này cho chắc cốp
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // 👇 Gắn Role vào Token. Chữ 'user.Role.ToString()' lúc này sẽ in ra "Admin" hoặc "Client"
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

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