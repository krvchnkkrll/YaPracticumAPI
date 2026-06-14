using Domain.Entities.Bookings;
using Domain.Enums;

namespace Persistence.Contracts.Repositories;

public interface IBookingRepository
{
    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    Task<Booking> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);

    /// <summary>
    ///     Получить брони с соответствующими статусами
    /// </summary>
    Task<IReadOnlyList<Booking>> GetBookingsByStatusesAsync(BookingStatus[] statuses,
        CancellationToken cancellationToken);
}