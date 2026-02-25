using NhatSoft.Domain.Entities;

namespace NhatSoft.Domain.Interfaces;

public interface IProjectRepository : IGenericRepository<Project>
{
    // Ví dụ hàm riêng: Lấy dự án kèm theo ảnh
    Task<Project?> GetBySlugWithImagesAsync(string slug);
}
