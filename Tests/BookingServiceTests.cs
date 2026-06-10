using Application.Contracts.Models;
using Application.Services;
using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;
using Persistence.Repositories;

namespace Tests;

public sealed class BookingServiceTests
{
    private Mock<IBookingRepository> MockBookingRepository { get; } = new();
    private Mock<IEventRepository> MockEventRepository { get; } = new();
    private BookingService CreateBookingService() => new(MockBookingRepository.Object, MockEventRepository.Object);

    [Fact]
    public void Create_NewBooking_ReturnCreatedPendingBooking()
    {
        var eventId = Guid.CreateVersion7();
        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        var service = CreateBookingService();
        var booking = service.CreateBookingAsync(eventId);

        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Create_ManyBookings_ReturnsPendingBookingsWithUniqueIds()
    {
        var eventId = Guid.CreateVersion7();
        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        const int bookingCount = 5;

        var service = CreateBookingService();
        var responses = new List<CreateBookingResponse>(capacity: bookingCount);

        for (var i = 0; i < bookingCount; i++)
            responses.Add(service.CreateBookingAsync(eventId));

        Assert.Equal(bookingCount, responses.Select(r => r.Id).Distinct().Count());
    }

    [Fact]
    public void Get_BookingById_ReturnCreatedBooking()
    {
        var eventId = Guid.CreateVersion7();
        var bookingsById = new Dictionary<Guid, Booking>();
        
        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) =>
            {
                var booking = Booking.Create(p);
                bookingsById[booking.Id] = booking;
                return booking;
            });
        
        MockBookingRepository
            .Setup(r => r.GetById(It.IsAny<Guid>()))
            .Returns((Guid bookingId) => bookingsById[bookingId]);

        var service = CreateBookingService();
        var created = service.CreateBookingAsync(eventId);
        var gotten = service.GetBookingByIdAsync(created.Id);

        Assert.Equal(created.Id, gotten.Id);
        Assert.Equal(eventId, gotten.EventId);
        Assert.Equal(nameof(BookingStatus.Pending), gotten.Status);
    }

    [Fact]
    public void Get_BookingAfterConfirm_ReturnsConfirmedBooking()
    {
        var eventId = Guid.CreateVersion7();
        var bookingsById = new Dictionary<Guid, Booking>();

        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) =>
            {
                var booking = Booking.Create(p);
                bookingsById[booking.Id] = booking;
                return booking;
            });
        
        MockBookingRepository
            .Setup(r => r.GetById(It.IsAny<Guid>()))
            .Returns((Guid bookingId) => bookingsById[bookingId]);

        var service = CreateBookingService();
        var created = service.CreateBookingAsync(eventId);
        bookingsById[created.Id].ConfirmBooking();

        var gotten = service.GetBookingByIdAsync(created.Id);

        Assert.Equal(nameof(BookingStatus.Confirmed), gotten.Status);
    }

    [Fact]
    public void Create_BookingForNotExistEvent_ThrowsKeyNotFoundException()
    {
        var parameters = new CreateBookingParameters
        {
            Id = Guid.CreateVersion7(),
            EventId = Guid.CreateVersion7()
        };
        
        var mockStorage = new Mock<IBookingStorage>();
        mockStorage
            .Setup(s => s.Bookings);
        
        MockEventRepository
            .Setup(r => r.GetEventById(It.IsAny<Guid>()))
            .Throws<KeyNotFoundException>();
        
        var repository = new BookingRepository(mockStorage.Object, MockEventRepository.Object);

        var service = new BookingService(repository, MockEventRepository.Object);

        FluentActions
            .Invoking(() => service.CreateBookingAsync(parameters.EventId))
            .Should()
            .Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Create_BookingForDeletedEvent_ThrowsKeyNotFoundException()
    {
        var parameters = new CreateBookingParameters
        {
            Id = Guid.CreateVersion7(),
            EventId = Guid.CreateVersion7()
        };
        
        var mockStorage = new Mock<IBookingStorage>();
        mockStorage
            .Setup(s => s.Bookings);
        
        MockEventRepository
            .Setup(r => r.GetEventById(It.IsAny<Guid>()))
            .Throws<KeyNotFoundException>();
        
        var repository = new BookingRepository(mockStorage.Object, MockEventRepository.Object);

        var service = new BookingService(repository, MockEventRepository.Object);

        FluentActions
            .Invoking(() => service.CreateBookingAsync(parameters.EventId))
            .Should()
            .Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Get_NotExistBooking_ThrowsKeyNotFoundException()
    {
        var missingBookingId = Guid.CreateVersion7();
        MockBookingRepository
            .Setup(r => r.GetById(missingBookingId))
            .Throws(new KeyNotFoundException());

        var service = CreateBookingService();

        FluentActions
            .Invoking(() => service.GetBookingByIdAsync(missingBookingId))
            .Should()
            .Throw<KeyNotFoundException>();
    }
}