using Application.Contracts.Models;
using Application.Contracts.Services;
using Domain.Exceptions;
using Persistence.Contracts;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IDbContext context) : IBookingService
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
    
    public async Task<CreateBookingResponse> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
    {
        try
        {
            await SemaphoreSlim.WaitAsync(cancellationToken);
            
            var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
           
            var reserveResult = eventEntity.TryReserveSeats();
            
            if (!reserveResult)
                throw new NoAvailableSeatsException();

            var booking = eventRepository.CreateBooking(eventEntity);
            
            await context.SaveChangesAsync(cancellationToken);
            
            return new CreateBookingResponse
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
            };
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<GetBookingResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        
        if (ReferenceEquals(booking, null))
            throw new KeyNotFoundException($"Бронь с идентификатором {bookingId} не найдено.");
        
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