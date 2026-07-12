namespace Application.Models;

public sealed class CreateUserRequest
{
    public required string Login { get; init; }
    public required string Password { get; set; }
}