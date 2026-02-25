using Microsoft.EntityFrameworkCore.Storage;
using NhatSoft.Domain.Entities;
using NhatSoft.Domain.Interfaces;
using NhatSoft.Infrastructure.Data;

namespace NhatSoft.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly NhatSoftDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // --- 1. KHAI BÁO BACKING FIELDS (Cần phải có các dòng này mới dùng được ??=) ---
    private IProjectRepository? _projects;
    private IGenericRepository<User>? _users;

    //  2 DÒNG NÀY 
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Post>? _posts;
    private IGenericRepository<Contact>? _contacts;
    private IGenericRepository<ProjectImage>? _projectImages;
    public UnitOfWork(NhatSoftDbContext context)
    {
        _context = context;
    }

    // --- 2. IMPLEMENT PROPERTIES ---

    // Projects (Repository riêng)
    public IProjectRepository Projects =>
        _projects ??= new ProjectRepository(_context);

    // Users (Generic)
    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    // Categories (Generic)
    public IGenericRepository<Category> Categories =>
        _categories ??= new GenericRepository<Category>(_context);

    // Posts (Generic)
    public IGenericRepository<Post> Posts =>
        _posts ??= new GenericRepository<Post>(_context);

    //  IMPLEMENT CONTACTS
    public IGenericRepository<Contact> Contacts =>
        _contacts ??= new GenericRepository<Contact>(_context);

    //  IMPLEMENT PROJECT IMAGES
    public IGenericRepository<ProjectImage> ProjectImages =>
        _projectImages ??= new GenericRepository<ProjectImage>(_context);
    // --- 3. CORE METHODS ---
    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // --- 4. TRANSACTION LOGIC (Giữ nguyên) ---
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
        GC.SuppressFinalize(this);
    }
}