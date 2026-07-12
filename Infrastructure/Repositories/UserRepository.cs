using Application.Interfaces.Repositories;
using Domain.Entities.Users;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserRepository(IDbContext context) : IUserRepository
{
    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public async Task<User?> GetByLoginAsync(string login, CancellationToken token)
    {
        return await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Login == login, token);
    }

    public async Task<User> GetReadOnlyByIdAsync(Guid userId, CancellationToken token)
    {
        return await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, token) ?? throw new KeyNotFoundException();
    }
    
    public async Task SaveChangesAsync(CancellationToken token)
    {
        await context.SaveChangesAsync(token);
    }
}