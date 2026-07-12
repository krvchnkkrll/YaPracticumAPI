using Application.Models;

namespace Application.Interfaces.Services;

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
}