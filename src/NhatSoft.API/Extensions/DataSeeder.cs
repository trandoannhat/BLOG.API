using Microsoft.EntityFrameworkCore;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Enums;
using NhatSoft.Infrastructure.Data;

namespace NhatSoft.API.Extensions;

public static class DataSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NhatSoftDbContext>();

        // ==========================================
        // BÍ QUYẾT ĐỂ CHẠY MƯỢT TRÊN VPS LÀ ĐÂY:
        // ==========================================
        // Lệnh này sẽ tự động tạo bảng (chạy Update-Database) nếu Database trắng
        await context.Database.MigrateAsync();

        // Kiểm tra xem trong DB đã có tài khoản Admin nào chưa
        var hasAdmin = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);

        if (!hasAdmin)
        {
            var adminUser = new User
            {
                FullName = "System Admin",
                Email = "doannhatit@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123@A"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            Console.WriteLine(" Đã tự động tạo tài khoản System Admin thành công!");
        }
    }
}