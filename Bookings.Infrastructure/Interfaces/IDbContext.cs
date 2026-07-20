using Bookings.Domain.Entities.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bookings.Infrastructure.Interfaces;

public interface IDbContext : IDisposable, IAsyncDisposable
{
    DbSet<Booking> Bookings { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    DatabaseFacade Database { get; }
}