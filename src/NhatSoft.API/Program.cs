using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NhatSoft.API.Middlewares;
using NhatSoft.Application; // Gọi DI của Application
using NhatSoft.Application.Settings;
using NhatSoft.Common.Wrappers;
using NhatSoft.Infrastructure; // Gọi DI của Infrastructure
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

        var response = new ApiResponse<string>(errors, "Dữ liệu không hợp lệ");
        return new BadRequestObjectResult(response);
    };
});

// B. GỌI CÁC TẦNG KIẾN TRÚC VÀO ĐÂY (Siêu gọn gàng)
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructure(builder.Configuration);

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
        Contact = new Microsoft.OpenApi.Models.OpenApiContact { Name = "NhatSoft", Email = "contact@nhatsoft.com" }
    });

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
// ==========================================
// TỰ ĐỘNG KHỞI TẠO DỮ LIỆU MẪU (DATA SEEDING)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Chạy hàm tạo Admin
        await NhatSoft.API.Extensions.DataSeeder.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, " Có lỗi xảy ra trong quá trình Seed Data.");
    }
}

// ============================================
// 4. MIDDLEWARE PIPELINE
// ============================================

app.UseMiddleware<ErrorHandlerMiddleware>();

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

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();