using Domain.Entities.Bookings;
using Domain.Entities.Users.Parameters;
using Domain.Enums;

namespace Domain.Entities.Users;

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
    
    private readonly List<Booking> _bookings = [];

    /// <summary>
    ///     Брони
    /// </summary>
    public IReadOnlyCollection<Booking> Bookings => _bookings;

    /// <summary>
    ///     Создать пользователя
    /// </summary>
    public static User Create(CreateUserParameter parameters) => new User(parameters);
}