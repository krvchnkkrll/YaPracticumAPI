using Bookings.Application.Models;

namespace Bookings.Application.Interfaces.Services;

public interface IBookingService
{
    /// <summary>
    ///     Добавить бронь 
    /// </summary>
    Task<CreateBookingResponse> CreateBookingAsync(Guid eventId, CancellationToken token);
    
    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    Task<GetBookingResponse> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken);
}