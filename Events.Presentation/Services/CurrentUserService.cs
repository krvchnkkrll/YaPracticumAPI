using System.Security.Claims;
using Events.Application.Interfaces.Identity;

namespace Events.Presentation.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId => Guid.Parse(
        httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public string Role => 
        httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Role)!;
}