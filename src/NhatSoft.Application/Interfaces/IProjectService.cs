

using NhatSoft.Application.DTOs.Project;
using NhatSoft.Common.Wrappers; // Để dùng PaginationFilter

namespace NhatSoft.Application.Interfaces;

public interface IProjectService
{
    // Hàm tạo dự án (có Transaction bên trong)
    Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto request);

    // Hàm lấy danh sách trả về kèm Tổng số bản ghi để phân trang
    Task<(IEnumerable<ProjectResponseDto> Data, int TotalRecords)> GetPagedProjectsAsync(ProjectFilterParams filter);

    Task<ProjectResponseDto> GetProjectByIdAsync(Guid id);

    Task UpdateProjectAsync(Guid id, UpdateProjectDto request);

    Task DeleteProjectAsync(Guid id);
    Task<ProjectResponseDto> GetProjectBySlugAsync(string slug);
}