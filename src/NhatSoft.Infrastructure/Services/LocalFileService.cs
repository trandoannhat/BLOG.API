using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.Infrastructure.Services
{
    public class LocalFileService(IWebHostEnvironment webHostEnvironment) : IFileService
    {
        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            // 1. Tạo đường dẫn lưu file: wwwroot/uploads/{folder}
            var rootPath = webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                // Fallback nếu không tìm thấy WebRootPath (môi trường đặc biệt)
                rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadsFolder = Path.Combine(rootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 2. Tạo tên file duy nhất (Tránh trùng tên)
            // Ví dụ: anh-meo.jpg -> anh-meo-GUID.jpg
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{fileName}-{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 3. Lưu file vào ổ cứng
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Trả về đường dẫn tương đối để Frontend truy cập
            // Kết quả: /uploads/projects/anh-meo-xxx.jpg
            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}