using Users.Domain.Enums;

namespace Users.Application.Models;

public sealed class CreateUserRequest
{
    public required string Login { get; init; }
    public required string Password { get; init; }
    public UserRoleEnum? Role { get; init; }
}