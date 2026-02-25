using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhatSoft.Application.DTOs.Project;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Wrappers;

namespace NhatSoft.API.Controllers;

public class ProjectsController(IProjectService projectService) : BaseApiController
{
    // ==========================================
    // 1. PUBLIC GROUP
    // ==========================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var (data, total) = await projectService.GetPagedProjectsAsync(filter);
        return Paged(data, filter.PageNumber, filter.PageSize, total, "GetProjects");
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await projectService.GetProjectByIdAsync(id);
        return Success(data, "Lấy dữ liệu thành công", "GetProjectById");
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var data = await projectService.GetProjectBySlugAsync(slug);
        return Success(data, "Lấy dữ liệu thành công", "GetProjectBySlug");
    }

    // ==========================================
    // 2. ADMIN GROUP
    // ==========================================

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProjectDto request)
    {
        var data = await projectService.CreateProjectAsync(request);

        // Code mới: Siêu gọn, trả về 201 Created chuẩn chỉnh
        return CreatedResource(data, "Tạo dự án thành công", "CreateProject");
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, UpdateProjectDto request)
    {
        if (id != request.Id)
            return Error("ID trên URL và Body không khớp", "UpdateError");

        await projectService.UpdateProjectAsync(id, request);
        return Success<string>(null, "Cập nhật thành công", "UpdateProject");
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await projectService.DeleteProjectAsync(id);
        return Success<string>(null, "Xóa dự án thành công", "DeleteProject");
    }
}