using Microsoft.EntityFrameworkCore;
using TutorDocs.Shared.Data;

namespace TutorDocs.Shared.Services;

public class UserProvider : IUserProvider
{
    private readonly TutorDocsDbContext _context;

    public UserProvider(TutorDocsDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new InvalidOperationException("No users found in the database. Please create a user first.");
        }

        return user.Id;
    }
}