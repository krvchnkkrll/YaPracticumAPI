using Application.Interfaces.Repositories;
using Domain.Entities.Users;
using Domain.Entities.Users.Parameters;
using Domain.Enums;
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

    protected async Task<Guid> CreateAndSaveUserAsync()
    {
        var user = User.Create(new CreateUserParameter
        {
            Login = Guid.NewGuid().ToString(),
            PasswordHash = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918",
            Role = UserRoleEnum.User
        });

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return user.Id;
    }
}