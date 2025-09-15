using TutorDocs.Shared.Repositories;

namespace TutorDocs.Shared.Services;

public interface IUnitOfWork : IDisposable
{
    IDocumentRepository DocumentRepository { get; }
    
    Task SaveChanges();
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
}