using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence.Contracts;

namespace Persistence;

internal sealed class DbContextFactory(IDbContextFactory<AppDbContext> factory) : IDbContextFactory
{
    public async Task<IDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await factory.CreateDbContextAsync(cancellationToken);
    }
}