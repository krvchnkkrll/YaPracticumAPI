using Domain.Entities.Bookings;

namespace Persistence.Contracts.Storages;

public interface IBookingStorage
{
    /// <summary>
    ///     Получить заложенные в памяти брони
    /// </summary>
    List<Booking> Bookings { get; }
}
