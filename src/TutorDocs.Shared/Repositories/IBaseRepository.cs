namespace TutorDocs.Shared.Repositories;

public interface IBaseRepository
{
    Task SaveChanges();
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
}