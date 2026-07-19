using Users.Domain.Enums;

namespace Users.Domain.Users.Parameters;

public struct CreateUserParameter
{
    /// <summary>
    ///     Логин
    /// </summary>
    public required string Login { get; init; }
    
    /// <summary>
    ///     Хэш пароля
    /// </summary>
    public required string PasswordHash { get; init; }
    
    /// <summary>
    ///     Роль
    /// </summary>
    public required UserRoleEnum Role { get; init; }
}