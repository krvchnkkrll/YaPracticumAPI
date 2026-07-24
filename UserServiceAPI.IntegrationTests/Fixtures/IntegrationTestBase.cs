using Users.Application.Interfaces.Repositories;
using Users.Infrastructure;
using Users.Infrastructure.Repositories;

namespace UserServiceAPI.IntegrationTests.Fixtures;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlFixture PostgreSqlFixture { get; set; } = null!;
    protected AppDbContext DbContext { get; set; } = null!;
    protected IUserRepository UserRepository { get; set; } = null!;

    public async Task InitializeAsync()
    {
        PostgreSqlFixture = new PostgreSqlFixture();
        await PostgreSqlFixture.InitializeAsync();

        DbContext = await PostgreSqlFixture.CreateDbContext();

        UserRepository = new UserRepository(DbContext);
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await PostgreSqlFixture.DisposeAsync();
    }
}