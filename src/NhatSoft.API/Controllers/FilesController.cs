using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.Interfaces;

namespace NhatSoft.API.Controllers
{
    // [Authorize] // Bỏ comment sau khi làm xong chức năng đăng nhập
    public class FilesController(IFileService fileService) : BaseApiController
    {
        /// <summary>
        /// Upload ảnh lên Cloudinary
        /// </summary>
        /// <param name="file">File ảnh</param>
        /// <param name="folder">Thư mục (vd: projects, blogs)</param>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, string folder = "general")
        {
            // Validate sơ bộ
            // SỬA: BadRequestResponse -> Error
            if (file == null) return Error("Chưa chọn file");

            // Check đuôi file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                // SỬA: BadRequestResponse -> Error
                return Error("Chỉ hỗ trợ định dạng ảnh (.jpg, .png, .webp)");

            // Check dung lượng (ví dụ 5MB)
            if (file.Length > 5 * 1024 * 1024)
                // SỬA: BadRequestResponse -> Error
                return Error("Dung lượng file tối đa là 5MB");

            // Gọi service upload
            var url = await fileService.UploadImageAsync(file, folder);

            // Trả về URL
            // SỬA: OkResponse -> Success
            return Success(new { url }, "Upload ảnh thành công", "Upload File");
        }
    }
}