using Domain.Enums;

namespace Domain.Entities.Users.Parameters;

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