using Domain.Entities.Bookings;
using Domain.Enums;

namespace Application.Interfaces.Repositories;

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

    /// <summary>
    ///     Удалить брони 
    /// </summary>
    void Remove(IEnumerable<Booking> bookings);

    /// <summary>
    ///     Получить бронь с мероприятием
    /// </summary>
    Task<Booking?> GetBookingOrDefaultIncludeEventAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<int> GetCountUserActiveBookingsAsync(Guid userId, CancellationToken cancellationToken);
    
    Task SaveChangesAsync(CancellationToken cancellationToken);
}