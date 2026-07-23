using Bookings.Application.Interfaces.Events;
using Bookings.Application.Interfaces.Identity;
using Bookings.Application.Interfaces.Repositories;
using Bookings.Application.Interfaces.Services;
using Bookings.Application.Models;
using Bookings.Application.Services;
using Bookings.Domain.Enums;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Static;
using Bookings.Infrastructure;
using Bookings.Infrastructure.Interfaces;
using Bookings.Infrastructure.Repositories;
using FluentAssertions;
using Kafka.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BookingServiceAPI.Tests;

public sealed class BookingServiceTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IBookingEventPublisher> _publisherMock = new();
    private readonly ServiceProvider _serviceProvider;

    public BookingServiceTests()
    {
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(_userId);
        currentUserServiceMock.Setup(x => x.Role).Returns(nameof(UserRoleEnum.User));

        _serviceProvider = BuildServiceProvider(_dbName, currentUserServiceMock.Object, _publisherMock.Object);
    }

    private static ServiceProvider BuildServiceProvider(
        string dbName,
        ICurrentUserService currentUserService,
        IBookingEventPublisher bookingEventPublisher)
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddSingleton(currentUserService);
        services.AddSingleton(bookingEventPublisher);
        services.AddSingleton<ILogger<BookingService>>(NullLogger<BookingService>.Instance);

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Create_NewBooking_ReturnCreatedPendingBooking()
    {
        var eventId = Guid.CreateVersion7();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var booking = await service.CreateBookingAsync(eventId, CancellationToken.None);

        booking.Id.Should().NotBeEmpty();
        booking.EventId.Should().Be(eventId);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task Create_ManyBookings_ReturnsPendingBookingsWithUniqueIds()
    {
        var eventId = Guid.CreateVersion7();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        const int bookingCount = 5;
        var responses = new List<CreateBookingResponse>(capacity: bookingCount);

        for (var i = 0; i < bookingCount; i++)
            responses.Add(await service.CreateBookingAsync(eventId, CancellationToken.None));

        responses.Should().HaveCount(bookingCount);
        responses.Select(r => r.Id).Should().OnlyHaveUniqueItems();
        responses.Should().OnlyContain(r => r.Status == BookingStatus.Pending);
    }

    [Fact]
    public async Task Get_BookingById_ReturnCreatedBooking()
    {
        var created = await CreateBookingAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var gotten = await service.GetBookingByIdAsync(created.Id, CancellationToken.None);

        gotten.Id.Should().Be(created.Id);
        gotten.EventId.Should().Be(created.EventId);
        gotten.Status.Should().Be(nameof(BookingStatus.Pending));
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
    public async Task Get_OtherUsersBooking_ThrowsBookingAccessDeniedException()
    {
        var created = await CreateBookingAsync();

        using var otherUserServiceProvider = BuildOtherUserServiceProvider(nameof(UserRoleEnum.User));
        using var scope = otherUserServiceProvider.CreateScope();
        var otherUserService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => otherUserService.GetBookingByIdAsync(created.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<BookingAccessDeniedException>();
    }

    [Fact]
    public async Task Get_OtherUsersBookingAsAdmin_ReturnsBooking()
    {
        var created = await CreateBookingAsync();

        using var adminServiceProvider = BuildOtherUserServiceProvider(nameof(UserRoleEnum.Admin));
        using var scope = adminServiceProvider.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var gotten = await adminService.GetBookingByIdAsync(created.Id, CancellationToken.None);

        gotten.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Delete_OwnPendingBooking_SetsStatusCancelledAndDoesNotPublish()
    {
        var created = await CreateBookingAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await service.DeleteBookingAsync(created.Id, CancellationToken.None);

        var gotten = await service.GetBookingByIdAsync(created.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));

        _publisherMock.Verify(
            p => p.PublishBookingCancelledAsync(It.IsAny<BookingCancelledEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_ConfirmedBooking_PublishesBookingCancelledEvent()
    {
        var created = await CreateBookingAsync();
        await ConfirmBookingAsync(created.Id);

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await service.DeleteBookingAsync(created.Id, CancellationToken.None);

        var gotten = await service.GetBookingByIdAsync(created.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));

        _publisherMock.Verify(
            p => p.PublishBookingCancelledAsync(
                It.Is<BookingCancelledEvent>(e =>
                    e.BookingId == created.Id &&
                    e.EventId == created.EventId &&
                    e.UserId == _userId &&
                    e.SeatsCount == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ConfirmedBooking_PublisherThrows_StillCancelsBooking()
    {
        var created = await CreateBookingAsync();
        await ConfirmBookingAsync(created.Id);

        _publisherMock
            .Setup(p => p.PublishBookingCancelledAsync(It.IsAny<BookingCancelledEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Kafka is down"));

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => service.DeleteBookingAsync(created.Id, CancellationToken.None))
            .Should()
            .NotThrowAsync();

        var gotten = await service.GetBookingByIdAsync(created.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));
    }

    [Fact]
    public async Task Delete_OtherUsersBooking_ThrowsBookingAccessDeniedException()
    {
        var created = await CreateBookingAsync();

        using var otherUserServiceProvider = BuildOtherUserServiceProvider(nameof(UserRoleEnum.User));
        using var scope = otherUserServiceProvider.CreateScope();
        var otherUserService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await FluentActions
            .Invoking(() => otherUserService.DeleteBookingAsync(created.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<BookingAccessDeniedException>();
    }

    [Fact]
    public async Task Delete_OtherUsersBookingAsAdmin_SetsStatusCancelled()
    {
        var created = await CreateBookingAsync();

        using var adminServiceProvider = BuildOtherUserServiceProvider(nameof(UserRoleEnum.Admin));
        using var scope = adminServiceProvider.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        await adminService.DeleteBookingAsync(created.Id, CancellationToken.None);

        var gotten = await adminService.GetBookingByIdAsync(created.Id, CancellationToken.None);
        gotten.Status.Should().Be(nameof(BookingStatus.Cancelled));
    }

    [Fact]
    public async Task Create_MoreThanLimitActiveBookings_ThrowsBookingLimitExceededException()
    {
        var eventId = Guid.CreateVersion7();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

        for (var i = 0; i < BookingConstance.MaximumActiveUserBookings; i++)
            await service.CreateBookingAsync(eventId, CancellationToken.None);

        await FluentActions
            .Invoking(() => service.CreateBookingAsync(eventId, CancellationToken.None))
            .Should()
            .ThrowAsync<BookingLimitExceededException>();
    }

    [Fact]
    public async Task Create_BookingLimitForDifferentUsers_DoesNotAffectEachOther()
    {
        var eventId = Guid.CreateVersion7();

        using (var scope = _serviceProvider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

            for (var i = 0; i < BookingConstance.MaximumActiveUserBookings; i++)
                await service.CreateBookingAsync(eventId, CancellationToken.None);
        }

        using var otherUserServiceProvider = BuildOtherUserServiceProvider(nameof(UserRoleEnum.User));
        using var otherUserScope = otherUserServiceProvider.CreateScope();
        var otherUserService = otherUserScope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await FluentActions
            .Invoking(() => otherUserService.CreateBookingAsync(eventId, CancellationToken.None))
            .Should()
            .NotThrowAsync();

        result.Subject.Status.Should().Be(BookingStatus.Pending);
    }

    private ServiceProvider BuildOtherUserServiceProvider(string role)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        mock.Setup(x => x.Role).Returns(role);

        return BuildServiceProvider(_dbName, mock.Object, _publisherMock.Object);
    }

    private async Task<CreateBookingResponse> CreateBookingAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
        return await service.CreateBookingAsync(Guid.CreateVersion7(), CancellationToken.None);
    }

    private async Task ConfirmBookingAsync(Guid bookingId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = await context.Bookings.SingleAsync(b => b.Id == bookingId);
        booking.ConfirmBooking();
        await context.SaveChangesAsync();
    }
}
