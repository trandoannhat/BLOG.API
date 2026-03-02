using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NhatSoft.Application.DTOs.Project;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Common.Helpers;
using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;
using System.Text.Json;

namespace NhatSoft.Application.Services;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{
    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto request)
    {
        var project = mapper.Map<Project>(request);

        if (request.TechStacks != null && request.TechStacks.Any())
        {
            project.TechStackJson = JsonSerializer.Serialize(request.TechStacks);
        }

        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            project.Images = new List<ProjectImage>();

            var thumbUrl = request.ThumbnailUrl;
            if (string.IsNullOrEmpty(thumbUrl))
            {
                thumbUrl = request.ImageUrls.First();
            }

            foreach (var url in request.ImageUrls)
            {
                project.Images.Add(new ProjectImage
                {
                    ImageUrl = url,
                    Caption = project.Name,
                    ProjectId = project.Id,
                    IsThumbnail = (url == thumbUrl)
                });
            }
        }

        if (string.IsNullOrEmpty(project.Slug))
        {
            project.Slug = request.Name.ToSlug();
        }

        await unitOfWork.Projects.AddAsync(project);
        await unitOfWork.CompleteAsync();

        return MapToResponse(project);
    }

    public async Task<(IEnumerable<ProjectResponseDto> Data, int TotalRecords)> GetPagedProjectsAsync(ProjectFilterParams filter)
    {
        // TỐI ƯU: Sử dụng Queryable thay vì lấy toàn bộ IEnumerable ra RAM
        var query = unitOfWork.Projects.GetAllQueryable()
                              .Include(p => p.Images) // 👇 Cần Include để map được Ảnh
                              .AsNoTracking();

        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(keyword)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(keyword)) ||
                (p.ClientName != null && p.ClientName.ToLower().Contains(keyword)) ||
                (p.TechStackJson != null && p.TechStackJson.ToLower().Contains(keyword))
            );
        }

        if (filter.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.StartDate >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.StartDate <= filter.ToDate.Value);
        }

        var totalRecords = await query.CountAsync();

        var pagedProjects = await query
                            .OrderByDescending(p => p.IsFeatured)
                            .ThenByDescending(p => p.CreatedAt)
                            .Skip((filter.PageNumber - 1) * filter.PageSize)
                            .Take(filter.PageSize)
                            .ToListAsync();

        var resultDto = pagedProjects.Select(MapToResponse);

        return (resultDto, totalRecords);
    }

    public async Task<ProjectResponseDto> GetProjectByIdAsync(Guid id)
    {
        // 👇 Cần Include Images để trả ảnh ra giao diện
        var project = await unitOfWork.Projects.GetAllQueryable()
                                     .Include(p => p.Images)
                                     .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            throw new NotFoundException("Không tìm thấy dự án");

        return MapToResponse(project);
    }

    public async Task<ProjectResponseDto> GetProjectBySlugAsync(string slug)
    {
        var project = await unitOfWork.Projects.GetAllQueryable()
                                     .Include(p => p.Images)
                                     .FirstOrDefaultAsync(p => p.Slug == slug);

        if (project == null)
            throw new NotFoundException($"Không tìm thấy dự án có slug: {slug}");

        return MapToResponse(project);
    }

    public async Task UpdateProjectAsync(Guid id, UpdateProjectDto request)
    {
        // 👇 Include Images để chuẩn bị thay thế ảnh
        var project = await unitOfWork.Projects.GetAllQueryable()
                                     .Include(p => p.Images)
                                     .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null)
            throw new NotFoundException("Không tìm thấy dự án để sửa");

        // Map đè
        mapper.Map(request, project);

        if (request.TechStacks != null)
        {
            project.TechStackJson = JsonSerializer.Serialize(request.TechStacks);
        }

        // TỐI ƯU CẬP NHẬT ẢNH TRÁNH LỖI ORPHANED
        if (request.ImageUrls != null)
        {
            // Xóa ảnh cũ trong CSDL
            if (project.Images != null && project.Images.Any())
            {
                unitOfWork.ProjectImages.DeleteRange(project.Images);
            }

            // Tạo list ảnh mới
            project.Images = new List<ProjectImage>();

            var thumbUrl = request.ThumbnailUrl;
            if (string.IsNullOrEmpty(thumbUrl) && request.ImageUrls.Any())
            {
                thumbUrl = request.ImageUrls.First();
            }

            foreach (var url in request.ImageUrls)
            {
                project.Images.Add(new ProjectImage
                {
                    ImageUrl = url,
                    ProjectId = id,
                    Caption = project.Name,
                    IsThumbnail = (url == thumbUrl)
                });
            }
        }

        unitOfWork.Projects.Update(project);
        await unitOfWork.CompleteAsync();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        var project = await unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
            throw new NotFoundException("Dự án không tồn tại");

        unitOfWork.Projects.Delete(project);
        await unitOfWork.CompleteAsync();
    }

    // ==========================================================
    // HELPER: MAP ENTITY -> RESPONSE DTO
    // ==========================================================
    private ProjectResponseDto MapToResponse(Project project)
    {
        var dto = mapper.Map<ProjectResponseDto>(project);

        // 1. Parse JSON TechStack
        if (!string.IsNullOrEmpty(project.TechStackJson))
        {
            try
            {
                dto.TechStacks = JsonSerializer.Deserialize<List<string>>(project.TechStackJson) ?? new List<string>();
            }
            catch
            {
                dto.TechStacks = new List<string>();
            }
        }

        // 2. Flatten Images (Tìm đúng ảnh Thumbnail)
        if (project.Images != null && project.Images.Any())
        {
            dto.ImageUrls = project.Images.Select(img => img.ImageUrl).ToList();

            // Tìm bức ảnh có IsThumbnail == true
            var thumbnailImg = project.Images.FirstOrDefault(img => img.IsThumbnail);

            if (thumbnailImg != null)
            {
                dto.ThumbnailUrl = thumbnailImg.ImageUrl;
            }
            else
            {
                dto.ThumbnailUrl = dto.ImageUrls.FirstOrDefault() ?? string.Empty;
            }
        }

        return dto;
    }
}