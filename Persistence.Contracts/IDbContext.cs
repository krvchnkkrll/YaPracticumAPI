using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Contracts;

public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Event> Events { get; }
    
    DbSet<Booking> Bookings { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}