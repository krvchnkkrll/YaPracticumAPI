using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Exceptions;
using Domain.Static;

namespace Application.Services;

internal sealed class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    ICurrentUserService currentUserService) : IBookingService
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
    
    public async Task<CreateBookingResponse> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        var currentUserId = currentUserService.UserId;

        var activeUserBookings = await bookingRepository.GetCountUserActiveBookingsAsync(currentUserId, token);

        if (activeUserBookings >= BookingConstance.MaximumActiveUserBookings)
            throw new BookingLimitExceededException(BookingConstance.MaximumActiveUserBookings);
        
        try
        {
            await SemaphoreSlim.WaitAsync(token);
            
            var eventEntity = await eventRepository.GetByIdAsync(eventId, token);
           
            var reserveResult = eventEntity.TryReserveSeats();
            
            if (!reserveResult)
                throw new NoAvailableSeatsException();

            var booking = eventRepository.CreateBooking(eventEntity, currentUserId);
            
            await eventRepository.SaveChangesAsync(token);
            
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