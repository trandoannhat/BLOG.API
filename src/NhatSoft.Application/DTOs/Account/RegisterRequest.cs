using System.ComponentModel.DataAnnotations;

namespace NhatSoft.Application.DTOs.Account;

public class RegisterRequest
{
    [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty; // Nên thêm cái này để UX tốt hơn
}
