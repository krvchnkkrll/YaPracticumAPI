using Bookings.Domain.Entities.Bookings;
using Bookings.Domain.Entities.Bookings.Parameters;
using Bookings.Domain.Enums;
using BookingServiceAPI.IntegrationTests.Fixtures;
using FluentAssertions;

namespace BookingServiceAPI.IntegrationTests;

public class BookingRepositoryTest : IntegrationTestBase
{
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsBooking()
    {
        var booking = await CreateAndSaveBookingAsync();

        var retrieved = await BookingRepository.GetByIdAsync(booking.Id, CancellationToken.None);

        retrieved.Should().BeEquivalentTo(booking);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        await FluentActions
            .Invoking(() => BookingRepository.GetByIdAsync(Guid.CreateVersion7(), CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdOrDefaultAsync_WithInvalidId_ReturnsNull()
    {
        var retrieved = await BookingRepository.GetByIdOrDefaultAsync(Guid.CreateVersion7(), CancellationToken.None);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetBookingsByStatuses_FilteredByStatuses_ReturnsBooking()
    {
        var confirmed = await CreateAndSaveBookingAsync();
        confirmed.ConfirmBooking();
        await CreateAndSaveBookingAsync();
        await CreateAndSaveBookingAsync();

        await BookingRepository.SaveChangesAsync(CancellationToken.None);

        var confirmedBookings =
            await BookingRepository.GetBookingsByStatusesAsync([BookingStatus.Confirmed], CancellationToken.None);

        confirmedBookings.Should().ContainSingle(b => b.Id == confirmed.Id);
    }

    [Fact]
    public async Task GetCountUserActiveBookingsAsync_CountsOnlyPendingAndConfirmed()
    {
        var userId = Guid.CreateVersion7();

        await CreateAndSaveBookingAsync(userId);
        var confirmed = await CreateAndSaveBookingAsync(userId);
        confirmed.ConfirmBooking();
        var cancelled = await CreateAndSaveBookingAsync(userId);
        cancelled.CancelledBooking();

        await BookingRepository.SaveChangesAsync(CancellationToken.None);

        var count = await BookingRepository.GetCountUserActiveBookingsAsync(userId, CancellationToken.None);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Remove_ExistingBooking_DeletesFromDatabase()
    {
        var booking = await CreateAndSaveBookingAsync();

        BookingRepository.Remove([booking]);
        await BookingRepository.SaveChangesAsync(CancellationToken.None);

        await FluentActions
            .Invoking(() => BookingRepository.GetByIdAsync(booking.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    private async Task<Booking> CreateAndSaveBookingAsync(Guid? userId = null)
    {
        var booking = Booking.Create(new CreateBookingParameters
        {
            EventId = Guid.CreateVersion7(),
            UserId = userId ?? Guid.CreateVersion7(),
        });

        BookingRepository.Add(booking);
        await BookingRepository.SaveChangesAsync(CancellationToken.None);

        return booking;
    }
}
