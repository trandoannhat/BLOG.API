using Microsoft.EntityFrameworkCore;
using NhatSoft.Domain.Entities.Base; // Sửa namespace BaseEntity
using NhatSoft.Domain.Interfaces;
using NhatSoft.Domain.Interfaces.Base;
using NhatSoft.Infrastructure.Data;
using System.Linq.Expressions;

namespace NhatSoft.Infrastructure.Repositories;

public class GenericRepository<T>(NhatSoftDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    // 1. Get Queryable (Đã tối ưu)
    public IQueryable<T> GetAllQueryable()
    {
        // Không cần check ISoftDelete ở đây nữa
        // Vì DbContext.OnModelCreating đã tự động gắn QueryFilter rồi
        return _dbSet.AsNoTracking().AsQueryable();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        => await _dbSet.Where(expression).ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    // 2. Xử lý Delete (Thông minh)
    public void Delete(T entity)
    {
        if (entity is ISoftDelete softDeleteEntity)
        {
            // Nếu là xóa mềm -> Set cờ và Update
            softDeleteEntity.IsDeleted = true;
            softDeleteEntity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            // Nếu không hỗ trợ xóa mềm (vd: ProjectImage) -> Xóa cứng
            _dbSet.Remove(entity);
        }
    }

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
}