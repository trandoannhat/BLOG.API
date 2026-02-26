using AutoMapper;
using NhatSoft.Application.DTOs.Blog;
using NhatSoft.Application.Interfaces;
using NhatSoft.Common.Exceptions;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;

namespace NhatSoft.Application.Services;

public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
{
    // 1. LẤY PHÂN TRANG (ADMIN)
    public async Task<(IEnumerable<CategoryDto> Data, int TotalRecords)> GetPagedCategoriesAsync(CategoryFilterParams filter)
    {
        // Lấy Queryable để tối ưu hiệu suất (nếu Repo hỗ trợ), ở đây giả sử dùng GetAllAsync trả về IEnumerable
        var query = await unitOfWork.Categories.GetAllAsync();

        // --- Filter ---
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            string keyword = filter.Keyword.ToLower().Trim();
            query = query.Where(c => c.Name.ToLower().Contains(keyword));
        }

        var totalRecords = query.Count();

        // --- Sort & Paging ---
        var pagedData = query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        // Map sang DTO (Bao gồm cả PostCount nếu Repo đã Include Posts)
        // Lưu ý: Để PostCount chính xác, Repository cần Include(c => c.Posts)
        return (mapper.Map<IEnumerable<CategoryDto>>(pagedData), totalRecords);
    }

    // 2. LẤY TẤT CẢ (DROPDOWN)
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await unitOfWork.Categories.GetAllAsync();
        return mapper.Map<IEnumerable<CategoryDto>>(categories.OrderBy(c => c.Name));
    }
    // --- THÊM MỚI 1: LẤY DANH MỤC DẠNG CÂY ---
    public async Task<IEnumerable<CategoryDto>> GetCategoryTreeAsync()
    {
        var categories = await unitOfWork.Categories.GetAllAsync();
        var dtos = mapper.Map<List<CategoryDto>>(categories.OrderBy(c => c.Name));

        var lookup = dtos.ToDictionary(c => c.Id);
        var tree = new List<CategoryDto>();

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && lookup.TryGetValue(dto.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
            }
            else
            {
                tree.Add(dto);
            }
        }

        return tree;
    }
    // 3. LẤY THEO ID
    public async Task<CategoryDto> GetByIdAsync(Guid id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) throw new NotFoundException("Danh mục không tồn tại");
        return mapper.Map<CategoryDto>(category);
    }

    // 4. LẤY THEO SLUG
    public async Task<CategoryDto> GetBySlugAsync(string slug)
    {
        // Cần đảm bảo Repository có hàm lấy theo điều kiện hoặc lấy hết rồi lọc
        var all = await unitOfWork.Categories.GetAllAsync();
        var category = all.FirstOrDefault(c => c.Slug == slug);

        if (category == null) throw new NotFoundException($"Không tìm thấy danh mục: {slug}");

        return mapper.Map<CategoryDto>(category);
    }

    // 5. TẠO MỚI
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto request)
    {
        // Check trùng tên (Optional)
        var all = await unitOfWork.Categories.GetAllAsync();
        if (all.Any(c => c.Name.ToLower() == request.Name.ToLower()))
        {
            throw new ApiException("Tên danh mục đã tồn tại");
        }
        // --- THÊM MỚI 2: Validate ParentId ---
        if (request.ParentId.HasValue && !all.Any(c => c.Id == request.ParentId.Value))
        {
            throw new NotFoundException("Danh mục cha không tồn tại.");
        }
        var category = mapper.Map<Category>(request);
        // Slug đã được tạo tự động trong AutoMapper Profile

        await unitOfWork.Categories.AddAsync(category);
        await unitOfWork.CompleteAsync();

        return mapper.Map<CategoryDto>(category);
    }

    // 6. CẬP NHẬT
    public async Task UpdateAsync(UpdateCategoryDto request)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category == null) throw new NotFoundException("Danh mục không tồn tại");
        // --- THÊM MỚI 3: Validate chặn vòng lặp cha-con ---
        if (request.ParentId == request.Id)
            throw new ApiException("Danh mục không thể tự làm cha của chính nó.");

        if (request.ParentId.HasValue && request.ParentId != category.ParentId)
        {
            var allCategories = await unitOfWork.Categories.GetAllAsync();
            bool isCircular = CheckCircularReferenceInMemory(request.Id, request.ParentId.Value, allCategories);
            if (isCircular)
                throw new ApiException("Không thể chọn danh mục con làm danh mục cha (Lỗi vòng lặp).");
        }
        // ----------------------------------------------------

        // Map dữ liệu mới vào
        mapper.Map(request, category);
        // Slug sẽ được cập nhật lại theo Name mới nhờ AutoMapper

        unitOfWork.Categories.Update(category);
        await unitOfWork.CompleteAsync();
    }

    // 7. XÓA
    public async Task DeleteAsync(Guid id)
    {
        var category = await unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) throw new NotFoundException("Danh mục không tồn tại");
        // --- THÊM MỚI 4: Xử lý an toàn khi xóa mềm (Bẻ gãy liên kết con) ---
        var allCategories = await unitOfWork.Categories.GetAllAsync();
        var children = allCategories.Where(c => c.ParentId == id).ToList();

        foreach (var child in children)
        {
            child.ParentId = null;
            unitOfWork.Categories.Update(child);
        }
        // -------------------------------------------------------------------
        // Check ràng buộc: Nếu danh mục đang có bài viết thì không cho xóa (Hoặc bắt buộc chuyển bài viết sang danh mục khác)
        // if (category.Posts.Any()) throw new ApiException("Danh mục đang chứa bài viết, không thể xóa!");

        unitOfWork.Categories.Delete(category);
        await unitOfWork.CompleteAsync();
    }
    // --- THÊM MỚI 5: Hàm hỗ trợ tìm vòng lặp ---
    private bool CheckCircularReferenceInMemory(Guid sourceId, Guid targetParentId, IEnumerable<Category> allCategories)
    {
        var lookup = allCategories.ToDictionary(c => c.Id);
        Guid? currentParentId = targetParentId;

        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == sourceId) return true; // Phát hiện vòng lặp
            if (lookup.TryGetValue(currentParentId.Value, out var parent))
            {
                currentParentId = parent.ParentId;
            }
            else break;
        }
        return false;
    }


}