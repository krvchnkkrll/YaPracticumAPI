using Bookings.Application.Interfaces.Identity;
using Bookings.Application.Interfaces.Services;
using Bookings.Domain.Entities.Bookings;
using Bookings.Domain.Enums;

namespace Bookings.Application.Services;

public sealed class AccessService(ICurrentUserService currentUserService) : IAccessService
{
    public bool IsCurrentUserHasAccessToBooking(Booking booking)
    {
        return booking.UserId == currentUserService.UserId 
               || 
               string.Equals(currentUserService.Role, nameof(UserRoleEnum.Admin), StringComparison.OrdinalIgnoreCase);
    }
}