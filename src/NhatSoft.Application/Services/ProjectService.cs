using AutoMapper;
using NhatSoft.Application.DTOs.Project;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Common.Wrappers;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;
using System.Text.Json;

namespace NhatSoft.Application.Services;

public class ProjectService(IUnitOfWork unitOfWork, IMapper mapper) : IProjectService
{
    // ==========================================================
    // 1. CREATE PROJECT
    // ==========================================================
    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto request)
    {
        // 1. Map cơ bản (Name, Description...)
        var project = mapper.Map<Project>(request);

        // 2. Xử lý TechStack (List<string> -> JSON String)
        if (request.TechStacks != null && request.TechStacks.Any())
        {
            project.TechStackJson = JsonSerializer.Serialize(request.TechStacks);
        }

        // 3. Xử lý Ảnh (List<string> -> List<ProjectImage>)
        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            project.Images = new List<ProjectImage>();
            foreach (var url in request.ImageUrls)
            {
                project.Images.Add(new ProjectImage
                {
                    ImageUrl = url,
                    Caption = project.Name // Caption mặc định là tên dự án
                });
            }
        }

        // 4. Tạo Slug (Nếu chưa có logic tự động thì generate đơn giản từ Name)
        // project.Slug = GenerateSlug(project.Name); // Bạn có thể thêm hàm helper này sau

        // 5. Lưu vào DB
        await unitOfWork.Projects.AddAsync(project);
        await unitOfWork.CompleteAsync();

        // 6. Trả về kết quả (Dùng hàm helper để map ngược lại cho đầy đủ)
        return MapToResponse(project);
    }

    // ==========================================================
    // 2. GET LIST (PAGINATION)
    // ==========================================================
    public async Task<(IEnumerable<ProjectResponseDto> Data, int TotalRecords)> GetPagedProjectsAsync(PaginationFilter filter)
    {
        // Lấy tất cả (Nếu data lớn nên dùng IQueryable, tạm thời dùng GetAllAsync của GenericRepo)
        var allProjects = await unitOfWork.Projects.GetAllAsync();

        // Tính tổng số bản ghi
        var totalRecords = allProjects.Count();

        // Phân trang trên RAM (Skip/Take)
        var pagedProjects = allProjects
                            .OrderByDescending(p => p.CreatedAt)
                            .Skip((filter.PageNumber - 1) * filter.PageSize)
                            .Take(filter.PageSize)
                            .ToList();

        // Map sang DTO (Dùng hàm helper để xử lý JSON và Image)
        var resultDto = pagedProjects.Select(MapToResponse);

        return (resultDto, totalRecords);
    }

    // ==========================================================
    // 3. GET BY ID
    // ==========================================================
    public async Task<ProjectResponseDto> GetProjectByIdAsync(Guid id)
    {
        var project = await unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
            throw new NotFoundException("Không tìm thấy dự án");

        return MapToResponse(project);
    }

    // ==========================================================
    // 4. GET BY SLUG
    // ==========================================================
    public async Task<ProjectResponseDto> GetProjectBySlugAsync(string slug)
    {
        // Ép kiểu sang IProjectRepository để gọi hàm riêng (GetBySlugWithImagesAsync)
        var projectRepo = unitOfWork.Projects as IProjectRepository;

        // Nếu ép kiểu thất bại (do chưa implement trong Repo) thì fallback về GetAll và lọc tay
        Project? project;
        if (projectRepo != null)
        {
            project = await projectRepo.GetBySlugWithImagesAsync(slug);
        }
        else
        {
            // Fallback: Lấy hết rồi lọc (Không khuyến khích nếu data lớn)
            var all = await unitOfWork.Projects.GetAllAsync();
            project = all.FirstOrDefault(p => p.Slug == slug);
        }

        if (project == null)
            throw new NotFoundException($"Không tìm thấy dự án có slug: {slug}");

        return MapToResponse(project);
    }

    // ==========================================================
    // 5. UPDATE PROJECT
    // ==========================================================
    public async Task UpdateProjectAsync(Guid id, UpdateProjectDto request)
    {
        var project = await unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
            throw new NotFoundException("Không tìm thấy dự án để sửa");

        // 1. Map dữ liệu cơ bản từ Request đè vào Entity cũ
        mapper.Map(request, project);

        // 2. Cập nhật JSON TechStack
        if (request.TechStacks != null)
        {
            project.TechStackJson = JsonSerializer.Serialize(request.TechStacks);
        }

        // 3. Cập nhật Ảnh (Chiến thuật: Xóa hết cũ -> Thêm mới)
        if (request.ImageUrls != null)
        {
            // Lưu ý: Project phải Include Images khi GetById thì mới Clear được
            // Nếu GenericRepo chưa Include, code này có thể chỉ thêm mới mà không xóa cũ được trọn vẹn
            // Tốt nhất nên dùng IProjectRepository để có hàm GetByIdWithImages

            if (project.Images == null) project.Images = new List<ProjectImage>();

            project.Images.Clear(); // Xóa ảnh cũ trong List

            foreach (var url in request.ImageUrls)
            {
                project.Images.Add(new ProjectImage
                {
                    ImageUrl = url,
                    ProjectId = id,
                    Caption = project.Name
                });
            }
        }

        unitOfWork.Projects.Update(project);
        await unitOfWork.CompleteAsync();
    }

    // ==========================================================
    // 6. DELETE PROJECT
    // ==========================================================
    public async Task DeleteProjectAsync(Guid id)
    {
        var project = await unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
            throw new NotFoundException("Dự án không tồn tại");

        // Gọi hàm Remove của Repository
        unitOfWork.Projects.Delete(project);
        await unitOfWork.CompleteAsync();
    }

    // ==========================================================
    // HELPER: MAP ENTITY -> RESPONSE DTO
    // ==========================================================
    // Hàm này giúp xử lý các logic mà AutoMapper mặc định không làm được
    // như: Parse JSON string thành List, Lấy URL từ bảng con Images
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

        // 2. Flatten Images (Lấy list URL từ bảng con)
        if (project.Images != null && project.Images.Any())
        {
            dto.ImageUrls = project.Images.Select(img => img.ImageUrl).ToList();

            // Lấy ảnh đầu tiên làm Thumbnail nếu chưa có
            if (string.IsNullOrEmpty(dto.ThumbnailUrl))
            {
                dto.ThumbnailUrl = dto.ImageUrls.FirstOrDefault() ?? string.Empty;
            }
        }

        return dto;
    }
}