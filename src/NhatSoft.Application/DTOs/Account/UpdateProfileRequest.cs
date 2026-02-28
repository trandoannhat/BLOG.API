namespace NhatSoft.Application.DTOs.Account;

public class UpdateProfileRequest
{
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
}
