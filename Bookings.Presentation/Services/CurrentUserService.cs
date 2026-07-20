using System.Security.Claims;
using Bookings.Application.Interfaces.Identity;

namespace Bookings.Presentation.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId => Guid.Parse(
        httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public string Role => 
        httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Role)!;
}