using Application.Contracts.Models;
using Application.Contracts.Services;
using Domain.Entities.Bookings.Parameters;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingService(IBookingRepository bookingRepository) : IBookingService
{
    public CreateBookingResponse CreateBookingAsync(Guid eventId)
    {
        var booking = bookingRepository.Create(new CreateBookingParameters
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId
        });

        return new CreateBookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
        };
    }

    public GetBookingResponse GetBookingAsync(Guid bookingId)
    {
        var booking = bookingRepository.GetById(bookingId);

        return new GetBookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
        };
    }
}