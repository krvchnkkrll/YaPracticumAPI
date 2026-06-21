using System.Reflection;
using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Persistence.Contracts;

namespace Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<Event> Events { get; set; }
    
    public DbSet<Booking> Bookings { get; set; }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}