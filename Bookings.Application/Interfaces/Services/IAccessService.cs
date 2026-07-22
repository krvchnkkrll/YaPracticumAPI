using Bookings.Domain.Entities.Bookings;

namespace Bookings.Application.Interfaces.Services;

public interface IAccessService
{
    /// <summary>
    ///     Текущий пользователь имеет доступ к брони
    /// </summary>
    bool IsCurrentUserHasAccessToBooking(Booking booking);
}