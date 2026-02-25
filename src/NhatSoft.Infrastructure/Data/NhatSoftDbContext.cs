using Microsoft.EntityFrameworkCore;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces.Base;
using System.Linq.Expressions;

namespace NhatSoft.Infrastructure.Data;

public class NhatSoftDbContext(DbContextOptions<NhatSoftDbContext> options) : DbContext(options)
{
    // 1. Khai báo các bảng (DbSet)
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectImage> ProjectImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    // --- CẤU HÌNH TỰ ĐỘNG NGÀY GIỜ & SOFT DELETE ---
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Xử lý Ngày tháng (IAuditable)
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Chặn không cho sửa ngày tạo
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    break;
            }
        }

        // 2. Xử lý Soft Delete (ISoftDelete) - Chặn lệnh xóa cứng
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // "Bẻ lái": Đang từ trạng thái Xóa (Deleted) -> chuyển thành Sửa (Modified)
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    // 2. Cấu hình chi tiết (Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Config User ---
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);

            // --- THÊM DÒNG NÀY: Lưu Role dạng chữ ---
            e.Property(x => x.Role).HasConversion<string>();
            // -----------------------------------------
        });

        // --- Config Project ---
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        // Quan hệ Project - ProjectImage
        modelBuilder.Entity<ProjectImage>(e =>
        {
            e.HasOne(i => i.Project)
             .WithMany(p => p.Images)
             .HasForeignKey(i => i.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Config Blog (Category & Post) ---
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);

            e.HasOne(p => p.Category)
             .WithMany(c => c.Posts)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Author)
             .WithMany(u => u.Posts)
             .HasForeignKey(p => p.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- TỰ ĐỘNG GÁN QUERY FILTER (WHERE IsDeleted = false) ---
        // Quét tất cả các Entity, bảng nào có ISoftDelete thì tự động thêm Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    // Hàm bổ trợ tạo Expression e => !e.IsDeleted
    private static LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var notDeleted = Expression.Not(property);
        // SỬA: Đã thêm dòng return thiếu ở code cũ
        return Expression.Lambda(notDeleted, parameter);
    }
}