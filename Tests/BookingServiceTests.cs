using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Static;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests;

public sealed class BookingServiceTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly ServiceProvider _serviceProvider;

    public BookingServiceTests()
    {
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(_userId);
        currentUserServiceMock.Setup(x => x.Role).Returns(nameof(UserRoleEnum.User));

        _serviceProvider = BuildServiceProvider(_dbName, currentUserServiceMock.Object);
    }

    private static ServiceProvider BuildServiceProvider(string dbName, ICurrentUserService currentUserService)
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddSingleton(currentUserService);

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Create_NewBooking_ReturnCreatedPendingBooking()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var booking = await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);

        booking.Id.Should().NotBeEmpty();
        booking.EventId.Should().Be(eventEntity.Id);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task Create_ManyBookings_ReturnsPendingBookingsWithUniqueIds()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        const int bookingCount = 5;
        var responses = new List<CreateBookingResponse>(capacity: bookingCount);

        for (var i = 0; i < bookingCount; i++)
            responses.Add(await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None));

        responses.Should().HaveCount(bookingCount);
        responses.Select(r => r.Id).Should().OnlyHaveUniqueItems();
        responses.Should().OnlyContain(r => r.Status == BookingStatus.Pending);
    }

    [Fact]
    public async Task Get_BookingById_ReturnCreatedBooking()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        CreateBookingResponse created;
        using (var scope = _serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
            created = await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
        }

        using var assertScope = _serviceProvider.CreateScope();
        var assertService = assertScope.ServiceProvider.GetRequiredService<IBookingService>();
        var gotten = await assertService.GetBookingByIdAsync(created.Id, CancellationToken.None);

        gotten.Id.Should().Be(created.Id);
        gotten.EventId.Should().Be(eventEntity.Id);
        gotten.Status.Should().Be(nameof(BookingStatus.Pending));
    }

    [Fact]
    public async Task Get_BookingAfterConfirm_ReturnsConfirmedBooking()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        var booking = await SeedBookingAsync(eventEntity.Id);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await context.Bookings.SingleAsync(b => b.Id == booking.Id);
            entity.ConfirmBooking();
            await context.SaveChangesAsync();
        }

        using var assertScope = _serviceProvider.CreateScope();
        var service = assertScope.ServiceProvider.GetRequiredService<IBookingService>();
        var gotten = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        gotten.Status.Should().Be(nameof(BookingStatus.Confirmed));
        gotten.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_BookingAfterReject_ReturnsRejectedBooking()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        var booking = await SeedBookingAsync(eventEntity.Id);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await context.Bookings.SingleAsync(b => b.Id == booking.Id);
            entity.RejectBooking();
            await context.SaveChangesAsync();
        }

        using var assertScope = _serviceProvider.CreateScope();
        var service = assertScope.ServiceProvider.GetRequiredService<IBookingService>();
        var gotten = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        gotten.Status.Should().Be(nameof(BookingStatus.Rejected));
        gotten.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReleaseSeats_AfterRejectedBooking_IncreasesAvailableSeats()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await context.Events.SingleAsync(e => e.Id == eventEntity.Id);

            entity.TryReserveSeats().Should().BeTrue();
            entity.AvailableSeats.Should().Be(4);

            var booking = entity.CreateBooking(_userId);
            booking.RejectBooking();
            entity.ReleaseSeats();

            await context.SaveChangesAsync();
        }

        using var assertScope = _serviceProvider.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actual = await assertContext.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);

        actual.AvailableSeats.Should().Be(5);
    }

    [Fact]
    public async Task Create_BookingForNotExistEvent_ThrowsKeyNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => service.CreateBookingAsync(Guid.CreateVersion7(), CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Create_BookingForDeletedEvent_ThrowsKeyNotFoundException()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Events.Remove(await context.Events.SingleAsync(e => e.Id == eventEntity.Id));
            await context.SaveChangesAsync();
        }

        using var assertScope = _serviceProvider.CreateScope();
        var service = assertScope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => service.CreateBookingAsync(eventEntity.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Get_NotExistBooking_ThrowsKeyNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => service.GetBookingByIdAsync(Guid.CreateVersion7(), CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Create_Booking_ReservesEventSeat()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using (var scope = _serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
        }

        using var assertScope = _serviceProvider.CreateScope();
        var context = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actual = await context.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);

        actual.AvailableSeats.Should().Be(4);
    }

    [Fact]
    public async Task Create_ManyBookings_ThrowsNoAvailableSeatsException()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(async () =>
            {
                for (var i = 0; i < 6; i++)
                    await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
            })
            .Should()
            .ThrowAsync<NoAvailableSeatsException>();
    }

    [Fact]
    public async Task Create_ConcurrentRequests_PreventsOverbooking()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        const int concurrentRequests = 20;

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                await start.Task;

                try
                {
                    await bookingService.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
            }))
            .ToArray();

        start.SetResult();

        var results = await Task.WhenAll(tasks);

        results.Count(success => success).Should().Be(5);
        results.Count(success => !success).Should().Be(15);

        using var assertScope = _serviceProvider.CreateScope();
        var context = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actual = await context.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);

        actual.AvailableSeats.Should().Be(0);
        (await context.Bookings.CountAsync()).Should().Be(5);
    }

    [Fact]
    public async Task Create_ConcurrentRequests_ReturnsBookingsWithUniqueIds()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 10);
        const int concurrentRequests = 10;

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                await start.Task;

                return await bookingService.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
            }))
            .ToArray();

        start.SetResult();

        var bookings = await Task.WhenAll(tasks);

        bookings.Should().HaveCount(10);
        bookings.Select(b => b.Id).Should().OnlyHaveUniqueItems();

        using var assertScope = _serviceProvider.CreateScope();
        var context = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var actual = await context.Events.AsNoTracking().SingleAsync(e => e.Id == eventEntity.Id);

        actual.AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task Create_BookingForPastEvent_ThrowsEventAlreadyStartedException()
    {
        var eventEntity = await SeedEventAsync(
            totalSeats: 5,
            startAt: DateTime.UtcNow.AddDays(-1),
            endAt: DateTime.UtcNow.AddDays(-1).AddHours(1));

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => service.CreateBookingAsync(eventEntity.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<EventAlreadyStartedException>();
    }

    [Fact]
    public async Task Create_MoreThanLimitActiveBookings_ThrowsBookingLimitExceededException()
    {
        var eventEntity = await SeedEventAsync(totalSeats: BookingConstance.MaximumActiveUserBookings + 5);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        for (var i = 0; i < BookingConstance.MaximumActiveUserBookings; i++)
            await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);

        await FluentActions
            .Invoking(() => service.CreateBookingAsync(eventEntity.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<BookingLimitExceededException>();
    }

    [Fact]
    public async Task Create_BookingLimitForDifferentUsers_DoesNotAffectEachOther()
    {
        var eventEntity = await SeedEventAsync(totalSeats: BookingConstance.MaximumActiveUserBookings + 5);

        using (var scope = _serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

            for (var i = 0; i < BookingConstance.MaximumActiveUserBookings; i++)
                await service.CreateBookingAsync(eventEntity.Id, CancellationToken.None);
        }

        var otherUserCurrentUserServiceMock = new Mock<ICurrentUserService>();
        otherUserCurrentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        otherUserCurrentUserServiceMock.Setup(x => x.Role).Returns(nameof(UserRoleEnum.User));

        using var otherUserServiceProvider = BuildServiceProvider(_dbName, otherUserCurrentUserServiceMock.Object);
        using var otherUserScope = otherUserServiceProvider.CreateScope();
        var otherUserService = otherUserScope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await FluentActions
            .Invoking(() => otherUserService.CreateBookingAsync(eventEntity.Id, CancellationToken.None))
            .Should()
            .NotThrowAsync();

        result.Subject.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task Delete_OwnBooking_SetsStatusCancelled()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        var booking = await SeedBookingAsync(eventEntity.Id);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await service.DeleteBookingAsync(booking.Id, CancellationToken.None);

        var gotten = await service.GetBookingByIdAsync(booking.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));
    }

    [Fact]
    public async Task Delete_OtherUsersBooking_ThrowsBookingAccessDeniedException()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        var booking = await SeedBookingAsync(eventEntity.Id);

        var otherUserCurrentUserServiceMock = new Mock<ICurrentUserService>();
        otherUserCurrentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        otherUserCurrentUserServiceMock.Setup(x => x.Role).Returns(nameof(UserRoleEnum.User));

        using var otherUserServiceProvider = BuildServiceProvider(_dbName, otherUserCurrentUserServiceMock.Object);
        using var otherUserScope = otherUserServiceProvider.CreateScope();
        var otherUserService = otherUserScope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => otherUserService.DeleteBookingAsync(booking.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<BookingAccessDeniedException>();
    }

    [Fact]
    public async Task Delete_OtherUsersBookingAsAdmin_SetsStatusCancelled()
    {
        var eventEntity = await SeedEventAsync(totalSeats: 5);
        var booking = await SeedBookingAsync(eventEntity.Id);

        var adminCurrentUserServiceMock = new Mock<ICurrentUserService>();
        adminCurrentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        adminCurrentUserServiceMock.Setup(x => x.Role).Returns(nameof(UserRoleEnum.Admin));

        using var adminServiceProvider = BuildServiceProvider(_dbName, adminCurrentUserServiceMock.Object);
        using var adminScope = adminServiceProvider.CreateScope();
        var adminService = adminScope.ServiceProvider.GetRequiredService<IBookingService>();

        await adminService.DeleteBookingAsync(booking.Id, CancellationToken.None);

        var gotten = await adminService.GetBookingByIdAsync(booking.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));
    }

    private async Task<Event> SeedEventAsync(int totalSeats = 5, DateTime? startAt = null, DateTime? endAt = null)
    {
        var eventEntity = Event.Create(new CreateEventParameter
        {
            Title = "Событие",
            Description = null,
            StartAt = startAt ?? DateTime.UtcNow.AddDays(1),
            EndAt = endAt ?? DateTime.UtcNow.AddDays(1).AddHours(1),
            TotalSeats = totalSeats
        });

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.Add(eventEntity);
        await context.SaveChangesAsync(CancellationToken.None);

        return eventEntity;
    }

    private async Task<Booking> SeedBookingAsync(Guid eventId)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var created = await service.CreateBookingAsync(eventId, CancellationToken.None);

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Bookings.SingleAsync(b => b.Id == created.Id);
    }
}
