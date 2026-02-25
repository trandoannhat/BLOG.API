using NhatSoft.Domain.Entities;

namespace NhatSoft.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IGenericRepository<User> Users { get; }
    
    IGenericRepository<Category> Categories { get; } 
    IGenericRepository<Post> Posts { get; }
    IGenericRepository<Contact> Contacts { get; }
    IGenericRepository<ProjectImage> ProjectImages { get; }
    // Sau này thêm các repo khác nếu phát sinh 
    Task<int> CompleteAsync(); // SaveChanges

    // --- Quản lý Transaction ---
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
