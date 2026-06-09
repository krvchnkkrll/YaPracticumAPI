using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;
using Persistence.Contracts.Storages;

namespace Persistence.Storages;

internal sealed class BookingStorage : IBookingStorage
{
    public List<Booking> Bookings { get; } = [];

    public BookingStorage(IEventStorage eventStorage)
    {
        var events = eventStorage.Events;

        var event1Id = events[0].Id;
        var event2Id = events[1].Id;

        for (var i = 0; i < 3; i++)
        {
            Bookings.Add(Booking.Create(new CreateBookingParameters
            {
                Id = Guid.CreateVersion7(),
                EventId = event1Id,
            }));
        }

        for (var i = 0; i < 2; i++)
        {
            Bookings.Add(Booking.Create(new CreateBookingParameters
            {
                Id = Guid.CreateVersion7(),
                EventId = event2Id,
            }));
        }
    }
}
