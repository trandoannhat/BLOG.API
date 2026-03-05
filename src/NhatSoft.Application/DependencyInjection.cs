using Microsoft.Extensions.DependencyInjection;
using NhatSoft.Application.Interfaces;
using NhatSoft.Application.Services;
using System.Reflection;

namespace NhatSoft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // 1. Đăng ký AutoMapper tự động quét profile
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // 2. Đăng ký toàn bộ các Business Services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IDonationService, DonationService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectImageService, ProjectImageService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IPartnerAdService, PartnerAdService>();

        return services;
    }
}