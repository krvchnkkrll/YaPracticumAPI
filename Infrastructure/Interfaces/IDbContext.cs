using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Interfaces;

public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Event> Events { get; }
    
    DbSet<Booking> Bookings { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    DatabaseFacade Database { get; }
}