using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;
using Persistence.Contracts.Repositories;
using Persistence.Storages;

namespace Persistence.Repositories;

internal sealed class BookingRepository(BookingStorage bookingStorage) : IBookingRepository
{
    public Booking GetById(Guid bookingId)
    {
        var eventToReturn = bookingStorage.Bookings.SingleOrDefault(b => b.Id == bookingId);
        
        if (ReferenceEquals(eventToReturn, null))
            throw new KeyNotFoundException($"Событие с идентификатором {bookingId} не найдено.");
        
        return eventToReturn;
    }

    public Booking Create(CreateBookingParameters parameters)
    {
        var booking = Booking.Create(parameters);
        
        bookingStorage.Bookings.Add(booking);
        
        return booking;
    }
}