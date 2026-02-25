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

        // Check ràng buộc: Nếu danh mục đang có bài viết thì không cho xóa (Hoặc bắt buộc chuyển bài viết sang danh mục khác)
        // if (category.Posts.Any()) throw new ApiException("Danh mục đang chứa bài viết, không thể xóa!");

        unitOfWork.Categories.Delete(category);
        await unitOfWork.CompleteAsync();
    }
}