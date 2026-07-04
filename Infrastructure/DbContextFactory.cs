using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

internal sealed class DbContextFactory(IDbContextFactory<AppDbContext> factory) : IDbContextFactory
{
    public async Task<IDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await factory.CreateDbContextAsync(cancellationToken);
    }
}