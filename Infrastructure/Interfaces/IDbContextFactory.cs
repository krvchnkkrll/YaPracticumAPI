namespace Infrastructure.Interfaces;

public interface IDbContextFactory
{
    Task<IDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}