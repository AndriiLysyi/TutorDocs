namespace TutorDocs.Shared.Services;

public interface IUserProvider
{
    Task<Guid> GetCurrentUserIdAsync();
}