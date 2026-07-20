using Events.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Events.Infrastructure.Interfaces;

public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Event> Events { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    DatabaseFacade Database { get; }
}