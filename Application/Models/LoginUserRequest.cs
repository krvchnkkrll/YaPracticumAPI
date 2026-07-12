namespace Application.Models;

public sealed class LoginUserRequest
{
    /// <summary>
    ///     Login
    /// </summary>
    public required string Login { get; init; }
    
    /// <summary>
    ///     Пароль
    /// </summary>
    public required string Password { get; init; }
}