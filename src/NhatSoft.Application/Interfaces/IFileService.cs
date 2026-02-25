using Microsoft.AspNetCore.Http;
namespace NhatSoft.Application.Interfaces;

public interface IFileService
{
    /// <summary>
    /// Upload file ảnh
    /// </summary>
    /// <param name="file">File từ Form gửi lên</param>
    /// <param name="folder">Thư mục muốn lưu (vd: projects, avatars)</param>
    /// <returns>Đường dẫn URL của ảnh</returns>
    Task<string> UploadImageAsync(IFormFile file, string folder);
}