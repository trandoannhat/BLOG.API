using AutoMapper;
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
    // ==========================================================
    // 1. CREATE PROJECT
    // ==========================================================
    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto request)
    {
        // 1. Map cơ bản
        var project = mapper.Map<Project>(request);

        // 2. Xử lý TechStack (List<string> -> JSON String)
        // Nếu bạn dùng DB lưu JSON string thì đoạn này chuẩn.
        if (request.TechStacks != null && request.TechStacks.Any())
        {
            project.TechStackJson = JsonSerializer.Serialize(request.TechStacks);
        }

        // 3. Xử lý Ảnh (QUAN TRỌNG ĐÃ FIX) 🛠️
        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            project.Images = new List<ProjectImage>(); // Hoặc project.ProjectImages tùy tên trong Entity

            // Nếu client có gửi ThumbnailUrl riêng, dùng nó. Nếu không, lấy ảnh đầu tiên.
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
                    ProjectId = project.Id, // Gán Id cha (dù EF tự hiểu nhưng gán cho chắc)

                    // 👇 LOGIC QUAN TRỌNG: Xác định ảnh nào là Thumbnail
                    IsThumbnail = (url == thumbUrl)
                });
            }
        }

        // 4. Tạo Slug (Dùng Helper extension method bạn đã có) 🛠️
        // Nếu Slug null hoặc rỗng thì tự generate từ Name
        if (string.IsNullOrEmpty(project.Slug))
        {
            project.Slug = request.Name.ToSlug(); // Hàm ToSlug() từ class StringHelper
        }

        // 5. Check trùng Slug (Optional - Best Practice)
        // var existingSlug = await unitOfWork.Projects.GetQueryable().AnyAsync(x => x.Slug == project.Slug);
        // if (existingSlug) project.Slug += "-" + DateTime.Now.Ticks;

        // 6. Lưu vào DB
        await unitOfWork.Projects.AddAsync(project);
        await unitOfWork.CompleteAsync();

        // 7. Map ngược lại (Nên dùng AutoMapper cho nhất quán)
        return mapper.Map<ProjectResponseDto>(project);
    }

    // ==========================================================
    // 2. GET LIST (PAGINATION + SEARCH)
    // ==========================================================
    //  Đổi tham số từ PaginationFilter -> ProjectFilterParams
    public async Task<(IEnumerable<ProjectResponseDto> Data, int TotalRecords)> GetPagedProjectsAsync(ProjectFilterParams filter)
    {
        // 1. Lấy dữ liệu
        // Lưu ý: Đang lấy IEnumerable (Ram), nếu sau này data lớn nên chuyển sang IQueryable (Database)
        var query = await unitOfWork.Projects.GetAllAsync();

        // 2. === LOGIC TÌM KIẾM TỪ KHÓA (KEYWORD) ===
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();

            query = query.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(keyword)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(keyword)) ||
                // 👇 Sửa lại ClientName: Cần check null và ToLower để tìm không phân biệt hoa thường
                (p.ClientName != null && p.ClientName.ToLower().Contains(keyword)) ||
                (p.TechStackJson != null && p.TechStackJson.ToLower().Contains(keyword))
            );
        }

        // 3. === LOGIC LỌC NÂNG CAO (MỚI) ===

        // Lọc theo Nổi bật
        if (filter.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
        }

        // Lọc theo Khoảng thời gian (StartDate)
        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.StartDate >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.StartDate <= filter.ToDate.Value);
        }

        // 4. Tính tổng số bản ghi (Sau khi lọc xong xuôi)
        var totalRecords = query.Count();

        // 5. Phân trang & Sắp xếp
        var pagedProjects = query
                            .OrderByDescending(p => p.IsFeatured) // Ưu tiên nổi bật lên đầu
                            .ThenByDescending(p => p.CreatedAt)   // Sau đó mới đến ngày tạo
                            .Skip((filter.PageNumber - 1) * filter.PageSize)
                            .Take(filter.PageSize)
                            .ToList();

        // 6. Map sang DTO
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