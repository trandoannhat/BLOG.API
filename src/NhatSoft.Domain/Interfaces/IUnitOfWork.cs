using NhatSoft.Domain.Entities;

namespace NhatSoft.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IGenericRepository<User> Users { get; }
    // Sau này thêm các repo khác nếu phát sinh : IPostRepository Posts { get; }

    Task<int> CompleteAsync(); // SaveChanges

    // --- Quản lý Transaction ---
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
