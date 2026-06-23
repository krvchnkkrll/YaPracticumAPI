using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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

    public async Task<AppDbContext> CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseSnakeCaseNamingConvention()
            .UseNpgsql(ConnectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName);
            });

        var context = new AppDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();

        return context;
    }
}