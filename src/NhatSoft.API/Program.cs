using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NhatSoft.API.Middlewares;
using NhatSoft.Application.Interfaces;
using NhatSoft.Application.Services;
using NhatSoft.Application.Settings;
using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Interfaces;
using NhatSoft.Infrastructure;
using NhatSoft.Infrastructure.Repositories;
using NhatSoft.Infrastructure.Services;
using System.Text;

// --- QUAN TRỌNG: Bật tính năng timestamp cho PostgreSQL ---
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. REGISTER SERVICES (Dependency Injection)
// ============================================

// A. Controllers & Validation Custom
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(new ApiResponse<string>(errors, "Dữ liệu không hợp lệ"));
    };
});

// B. Core Services & Infrastructure
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddInfrastructure(builder.Configuration); // DbContext nằm trong này

// C. UnitOfWork & Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// builder.Services.AddScoped<IProjectRepository, ProjectRepository>(); // Không cần thiết nếu đã dùng UnitOfWork, nhưng để cũng không sao

// D. Domain Services (Business Logic)
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAccountService, AccountService>(); // Đăng ký AccountService

// E. File Service (CHỌN 1 TRONG 2)
// Cách 1: Lưu ảnh trên Server (Local)
//builder.Services.AddScoped<IFileService, LocalFileService>();
// Cách 2: Lưu ảnh trên Cloudinary (Bỏ comment dòng dưới nếu muốn dùng Cloud)
builder.Services.AddScoped<IFileService, CloudinaryFileService>();

// ============================================
// 2. CONFIG AUTHENTICATION (JWT)
// ============================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
    };
});

// ============================================
// 3. CONFIG SWAGGER & CORS
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NhatSoft API Core",
        Version = "v1",
        Description = "Hệ thống Backend quản lý Blog & Portfolio",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "NhatSoft Support",
            Email = "contact@nhatsoft.com"
        }
    });

    // Cấu hình nút Ổ khóa (Authorize)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token JWT vào đây."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ============================================
// 4. MIDDLEWARE PIPELINE (Thứ tự cực quan trọng)
// ============================================

// 1. Error Handler (Luôn đầu tiên)
app.UseMiddleware<ErrorHandlerMiddleware>();

// 2. Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NhatSoft API v1");
        c.RoutePrefix = string.Empty;
        c.DefaultModelsExpandDepth(-1);
    });
}

// 3. Static Files (Để xem ảnh)
app.UseStaticFiles();

// 4. HTTPS & CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 5. AUTHENTICATION & AUTHORIZATION (Phải đúng thứ tự này)
app.UseAuthentication(); // <--- ĐÃ THÊM: Kiểm tra "Bạn là ai?" (Check Token)
app.UseAuthorization();  // <--- Kiểm tra "Bạn được làm gì?" (Check Role)

// 6. Map Controllers
app.MapControllers();

app.Run();