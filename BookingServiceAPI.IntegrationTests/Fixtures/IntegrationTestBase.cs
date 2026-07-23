using Bookings.Application.Interfaces.Repositories;
using Bookings.Infrastructure;
using Bookings.Infrastructure.Repositories;

namespace BookingServiceAPI.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlFixture PostgreSqlFixture { get; set; } = null!;
    protected AppDbContext DbContext { get; set; } = null!;
    protected IBookingRepository BookingRepository { get; set; } = null!;

    public async Task InitializeAsync()
    {
        PostgreSqlFixture = new PostgreSqlFixture();
        await PostgreSqlFixture.InitializeAsync();

        DbContext = await PostgreSqlFixture.CreateDbContext();

        BookingRepository = new BookingRepository(DbContext);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await PostgreSqlFixture.DisposeAsync();
    }
}
