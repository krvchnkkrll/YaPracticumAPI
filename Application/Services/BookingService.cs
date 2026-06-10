using Application.Contracts.Models;
using Application.Contracts.Services;
using Domain.Entities.Bookings.Parameters;
using Domain.Exceptions;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository) : IBookingService
{
    private readonly Lock _bookingLock = new();
    
    public CreateBookingResponse CreateBookingAsync(Guid eventId)
    {
        lock (_bookingLock)
        {
            var eventEntity = eventRepository.GetById(eventId);

            var result = eventEntity.TryReserveSeats();

            if (!result)
                throw new NoAvailableSeatsException();
            
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
    }

    public GetBookingResponse GetBookingByIdAsync(Guid bookingId)
    {
        var booking = bookingRepository.GetById(bookingId);
        
        return new GetBookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}