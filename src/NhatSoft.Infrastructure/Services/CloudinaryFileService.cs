using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions; // Để ném lỗi nếu upload thất bại
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NhatSoft.Infrastructure.Services
{
    public class CloudinaryFileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileService(IConfiguration config)
        {
            // Lấy thông tin cấu hình từ appsettings.json
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Chưa cấu hình Cloudinary trong appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Luôn dùng HTTPS
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ValidationException("File không được để trống");
            }

            // Đọc file thành stream
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                // Cấu trúc folder trên Cloud: NhatSoft/projects hoặc NhatSoft/avatars
                Folder = $"NhatSoft/{folder}",

                // Tự động đặt tên file ngẫu nhiên để không bị trùng
                PublicId = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}",

                // Tự động overwrite nếu trùng tên (dù đã có Guid nhưng cứ set cho chắc)
                Overwrite = true
            };

            // Thực hiện upload
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Kiểm tra kết quả
            if (uploadResult.Error != null)
            {
                throw new Exception($"Lỗi upload ảnh lên Cloudinary: {uploadResult.Error.Message}");
            }

            // Trả về đường dẫn ảnh tuyệt đối (https://...)
            return uploadResult.SecureUrl.ToString();
        }
    }
}