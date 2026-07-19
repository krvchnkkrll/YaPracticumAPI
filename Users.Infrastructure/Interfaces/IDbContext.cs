using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Users.Domain.Users;

namespace Users.Infrastructure.Interfaces;

public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<User> Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    DatabaseFacade Database { get; }
}