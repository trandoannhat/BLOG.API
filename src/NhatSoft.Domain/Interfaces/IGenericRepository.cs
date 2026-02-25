using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Entities.Base;
using System.Linq.Expressions;

namespace NhatSoft.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();

    // ---  QUAN TRỌNG ---
    IQueryable<T> GetAllQueryable(); // Trả về câu lệnh SQL chưa chạy (để nối thêm Where, Skip, Take...)
    // ---------------------------

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

    Task AddAsync(T entity);

    // Bổ sung thêm cho tiện (Option)
    Task AddRangeAsync(IEnumerable<T> entities);
    void DeleteRange(IEnumerable<T> entities);

    void Update(T entity);
    void Delete(T entity);
}