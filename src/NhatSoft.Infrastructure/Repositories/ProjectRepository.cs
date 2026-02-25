using Microsoft.EntityFrameworkCore;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;
using NhatSoft.Infrastructure.Data;

namespace NhatSoft.Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(NhatSoftDbContext context) : base(context)
        {
        }

        // Triển khai hàm lấy Slug kèm Ảnh
        public async Task<Project?> GetBySlugWithImagesAsync(string slug)
        {
            return await _dbSet
                .Include(p => p.Images) // JOIN bảng Images
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }
    }
}