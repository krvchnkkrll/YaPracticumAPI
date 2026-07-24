using Users.Domain.Enums;
using Users.Domain.Users.Parameters;

namespace Users.Domain.Users;

public sealed class User
{
    private User() { }

    private User(CreateUserParameter parameters) : this()
    {
        Login = parameters.Login;
        PasswordHash = parameters.PasswordHash;
        Role = parameters.Role;
    }

    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    
    /// <summary>
    ///     Логин
    /// </summary>
    public string Login { get; private set; } = null!;

    /// <summary>
    ///     Хэш пароля
    /// </summary>
    public string PasswordHash { get; private set; } = null!;
    
    /// <summary>
    ///     Роль
    /// </summary>
    public UserRoleEnum Role { get; private set; }

    /// <summary>
    ///     Создать пользователя
    /// </summary>
    public static User Create(CreateUserParameter parameters) => new User(parameters);
}