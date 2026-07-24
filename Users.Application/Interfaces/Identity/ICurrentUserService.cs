namespace Users.Application.Interfaces.Identity;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
}