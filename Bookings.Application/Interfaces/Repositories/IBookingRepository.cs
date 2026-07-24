using Bookings.Domain.Entities.Bookings;
using Bookings.Domain.Enums;

namespace Bookings.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    /// <summary>
    ///     Добавить бронь
    /// </summary>
    void Add(Booking booking);

    /// <summary>
    ///     Получить бронь по идентификатору
    /// </summary>
    Task<Booking> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);

    /// <summary>
    ///     Получить брони с соответствующими статусами
    /// </summary>
    Task<IReadOnlyList<Booking>> GetBookingsByStatusesAsync(BookingStatus[] statuses,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Удалить брони 
    /// </summary>
    void Remove(IEnumerable<Booking> bookings);

    /// <summary>
    ///     Получить бронь по идентификатору или null
    /// </summary>
    Task<Booking?> GetByIdOrDefaultAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<int> GetCountUserActiveBookingsAsync(Guid userId, CancellationToken cancellationToken);
    
    Task SaveChangesAsync(CancellationToken cancellationToken);
}