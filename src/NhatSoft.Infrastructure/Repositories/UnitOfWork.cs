using Microsoft.EntityFrameworkCore.Storage;
using NhatSoft.Domain.Entities; // <--- Nhớ thêm using này để hiểu "User"
using NhatSoft.Domain.Interfaces;
using NhatSoft.Infrastructure.Data;

namespace NhatSoft.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly NhatSoftDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Cache repositories
    private IProjectRepository? _projects;
    // 1. Thêm biến backing field cho Users
    private IGenericRepository<User>? _users;

    public UnitOfWork(NhatSoftDbContext context)
    {
        _context = context;
    }

    // Lazy Loading Repository: Projects
    public IProjectRepository Projects
    {
        get { return _projects ??= new ProjectRepository(_context); }
    }

    // 2. Thêm Property Users (Lazy Loading)
    // Nếu _users null thì new GenericRepository, nếu có rồi thì trả về luôn
    public IGenericRepository<User> Users
    {
        get { return _users ??= new GenericRepository<User>(_context); }
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // --- Transaction Logic (Giữ nguyên không đổi) ---

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}