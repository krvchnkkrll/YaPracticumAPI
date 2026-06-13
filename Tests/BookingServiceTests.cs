using Application.Contracts.Models;
using Application.Services;
using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;
using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Domain.Enums;
using Domain.Exceptions;
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
        
        CreateTestEvent(eventId);
        
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
        
        CreateTestEvent(eventId);
        
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
        
        CreateTestEvent(eventId);
        
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
        CreateTestEvent(eventId);
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
        Assert.NotNull(gotten.ProcessedAt);
    }
    
    [Fact]
    public void Get_BookingAfterReject_ReturnsRejectedBooking()
    {
        var eventId = Guid.CreateVersion7();
        var bookingsById = new Dictionary<Guid, Booking>();

        CreateTestEvent(eventId);

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
        bookingsById[created.Id].RejectBooking();

        var gotten = service.GetBookingByIdAsync(created.Id);

        Assert.Equal(nameof(BookingStatus.Rejected), gotten.Status);
        Assert.NotNull(gotten.ProcessedAt);
    }
    
    [Fact]
    public void Get_ReleaseSeats_IncreasesAvailableSeats()
    {
        var eventId = Guid.CreateVersion7();
        var eventEntity = CreateTestEvent(eventId);
        
        var startedEventAvailableSeats = eventEntity.AvailableSeats;
        eventEntity.TryReserveSeats();
        eventEntity.ReleaseSeats();
        
        Assert.Equal(startedEventAvailableSeats, eventEntity.AvailableSeats);
    }
    
    [Fact]
    public void Get_ReleaseSeats_AfterRejectSeats()
    {
        var eventId = Guid.CreateVersion7();
        var eventEntity = CreateTestEvent(eventId);
        
        var startedEventAvailableSeats = eventEntity.AvailableSeats;
        eventEntity.TryReserveSeats();
        eventEntity.ReleaseSeats();
        eventEntity.TryReserveSeats();
        
        Assert.Equal(startedEventAvailableSeats, eventEntity.AvailableSeats + 1);
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
    
    [Fact]
    public void Create_Booking_ReserveEventSeats()
    {
        var eventId = Guid.CreateVersion7();
        
        var eventEntity = CreateTestEvent(eventId);
        var availableSeatsBeforeBooking = eventEntity.AvailableSeats;
        
        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        var service = CreateBookingService();
        service.CreateBookingAsync(eventId);
        
        var availableSeatsAfterBooking = eventEntity.AvailableSeats;

        Assert.Equal(availableSeatsBeforeBooking, availableSeatsAfterBooking + 1);
    }
    
    [Fact]
    public void Create_ManyBookings_ThrowsNoAvailableSeatsException()
    {
        var eventId = Guid.CreateVersion7();
        
        CreateTestEvent(eventId);
        
        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        const int bookingCount = 6;

        var service = CreateBookingService();
        
        FluentActions
            .Invoking(() =>
            {
                for (var i = 0; i < bookingCount; i++)
                {
                    service.CreateBookingAsync(eventId);
                }
            })
            .Should()
            .Throw<NoAvailableSeatsException>();
    }
    
    [Fact]
    public async Task Create_ConcurrentRequests_PreventsOverbooking()
    {
        var eventId = Guid.CreateVersion7();
        var eventEntity = CreateTestEvent(eventId, totalSeats: 5);

        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        var service = CreateBookingService();

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                await start.Task;

                try
                {
                    service.CreateBookingAsync(eventId);
                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
            })
            .ToArray();

        start.SetResult();

        var results = await Task.WhenAll(tasks);

        results.Count(success => success)
            .Should()
            .Be(5);

        results.Count(success => !success)
            .Should()
            .Be(15);

        eventEntity.AvailableSeats
            .Should()
            .Be(0);
    }
    
    [Fact]
    public async Task Create_ConcurrentRequests_ReturnsBookingsWithUniqueIds()
    {
        var eventId = Guid.CreateVersion7();
        var eventEntity = CreateTestEvent(eventId, totalSeats: 10);

        MockBookingRepository
            .Setup(r => r.Create(It.IsAny<CreateBookingParameters>()))
            .Returns((CreateBookingParameters p) => Booking.Create(p));

        var service = CreateBookingService();

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, 10)
            .Select(async _ =>
            {
                await start.Task;
                return service.CreateBookingAsync(eventId);
            })
            .ToArray();

        start.SetResult();

        var bookings = await Task.WhenAll(tasks);

        bookings
            .Should()
            .HaveCount(10);

        bookings
            .Select(b => b.Id)
            .Should()
            .OnlyHaveUniqueItems();

        eventEntity.AvailableSeats
            .Should()
            .Be(0);
    }
    
    private Event CreateTestEvent(Guid eventId, int totalSeats = 5)
    {
        var eventEntity = Event.Create(new CreateEventParameter
        {
            Id = eventId,
            Title = "Событие",
            Description = null,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1).AddHours(1),
            TotalSeats = totalSeats
        });

        MockEventRepository
            .Setup(r => r.GetEventById(eventId))
            .Returns(eventEntity);

        return eventEntity;
    }
}