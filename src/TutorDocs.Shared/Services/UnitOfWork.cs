using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Repositories;

namespace TutorDocs.Shared.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly TutorDocsDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private IDocumentRepository? _documentRepository;
    private bool _disposed = false;

    public UnitOfWork(TutorDocsDbContext context)
    {
        _context = context;
    }

    public IDocumentRepository DocumentRepository 
    { 
        get { return _documentRepository ??= new DocumentRepository(_context); } 
    }

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }

    public async Task BeginTransaction()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _currentTransaction.CommitAsync();
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransaction()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to rollback.");
        }

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            _disposed = true;
        }
    }
}