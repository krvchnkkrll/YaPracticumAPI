using Events.Application.Interfaces.Repositories;
using Events.Infrastructure;
using Events.Infrastructure.Repositories;

namespace EventServiceAPI.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlFixture PostgreSqlFixture { get; set; } = null!;
    protected AppDbContext DbContext { get; set; } = null!;
    protected IEventRepository EventRepository { get; set; } = null!;

    public async Task InitializeAsync()
    {
        PostgreSqlFixture = new PostgreSqlFixture();
        await PostgreSqlFixture.InitializeAsync();

        DbContext = await PostgreSqlFixture.CreateDbContext();

        EventRepository = new EventRepository(DbContext);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await PostgreSqlFixture.DisposeAsync();
    }
}
