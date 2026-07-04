using Application.Interfaces.Repositories;
using Infrastructure;
using Infrastructure.Repositories;

namespace IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlFixture PostgreSqlFixture { get; set; } = null!;
    protected AppDbContext DbContext { get; set; } = null!;
    protected IEventRepository EventRepository { get; set; } = null!;
    protected IBookingRepository BookingRepository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PostgreSqlFixture = new PostgreSqlFixture();
        await PostgreSqlFixture.InitializeAsync();

        DbContext = await PostgreSqlFixture.CreateDbContext();

        EventRepository = new EventRepository(DbContext);
        BookingRepository = new BookingRepository(DbContext);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await PostgreSqlFixture.DisposeAsync();
    }
}