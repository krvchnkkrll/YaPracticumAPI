using Application.Contracts.Models;

namespace Application.Contracts.Services;

public interface IBookingService
{
    /// <summary>
    ///     Добавить бронь 
    /// </summary>
    CreateBookingResponse CreateBookingAsync(Guid eventId);
    
    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    GetBookingResponse GetBookingByIdAsync(Guid bookingId);
}