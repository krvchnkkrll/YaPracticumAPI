using Microsoft.EntityFrameworkCore;
using Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Fixtures;

internal sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;

    private string ConnectionString { get; set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        var context = new AppDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated();

        return context;
    }
}