using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Domain.Enums;
using FluentAssertions;
using IntegrationTests.Fixtures;

namespace IntegrationTests;

public class BookingRepositoryTest : IntegrationTestBase
{
    private readonly CreateEventParameter _eventParameter = new CreateEventParameter
    {
        Title = "Test1",
        Description = "ForIntegrationsTests",
        StartAt = DateTime.UtcNow.TruncateToMicroseconds(),
        EndAt = DateTime.UtcNow.AddDays(1).TruncateToMicroseconds(),
        TotalSeats = 10
    };
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsBooking()
    {
        var eventEntity = await CreateAndSaveEventAsync();
        eventEntity.TryReserveSeats();
        var booking = EventRepository.CreateBooking(eventEntity);
        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var retrieved = await BookingRepository.GetByIdAsync(booking.Id, CancellationToken.None);
        retrieved.Should().BeEquivalentTo(booking);
    }

    [Fact]
    public async Task GetBookingByStatuses_FilteredByStatuses_ReturnsBooking()
    {
        var eventEntity = await CreateAndSaveEventAsync();
        eventEntity.TryReserveSeats();
        var booking = EventRepository.CreateBooking(eventEntity);
        booking.ConfirmBooking();
        EventRepository.CreateBooking(eventEntity);
        EventRepository.CreateBooking(eventEntity);

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var confirmedBookings = await BookingRepository.GetBookingsByStatusesAsync([BookingStatus.Confirmed], CancellationToken.None);
        
        confirmedBookings.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task Remove_ExistingBooking_DeletesFromDatabase()
    {
        var eventEntity = await CreateAndSaveEventAsync();
        eventEntity.TryReserveSeats();
        var booking = EventRepository.CreateBooking(eventEntity);
        booking.ConfirmBooking();

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        BookingRepository.Remove([booking]);
        await BookingRepository.SaveChangesAsync(CancellationToken.None);
        
        await FluentActions
            .Invoking(() => BookingRepository.GetByIdAsync(booking.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }
    
    
    private async Task<Event> CreateAndSaveEventAsync()
    {
        var eventEntity = Event.Create(_eventParameter);
        EventRepository.Add(eventEntity);
        await EventRepository.SaveChangesAsync(CancellationToken.None);
        return eventEntity;
    }
}
