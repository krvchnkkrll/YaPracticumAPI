using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;

namespace Persistence.Contracts.Repositories;

public interface IBookingRepository
{
    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    Booking GetById(Guid bookingId);
    
    /// <summary>
    ///     Добавить бронь
    /// </summary>
    Booking Create(CreateBookingParameters parameters);
}