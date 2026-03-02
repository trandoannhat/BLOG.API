using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NhatSoft.Application.Interfaces;
using NhatSoft.Domain.Interfaces;
using NhatSoft.Infrastructure.Data;
using NhatSoft.Infrastructure.Repositories;
using NhatSoft.Infrastructure.Services;

namespace NhatSoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Đăng ký DbContext (PostgreSQL)
        services.AddDbContext<NhatSoftDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // 2. Đăng ký UnitOfWork (Đã bao hàm tất cả Repositories)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 3. Đăng ký các dịch vụ Hạ tầng khác (File, Cloud...)
        services.AddScoped<IFileService, CloudinaryFileService>();

        return services;
    }
}