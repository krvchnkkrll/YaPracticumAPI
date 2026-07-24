namespace Users.Application.Interfaces.Identity;

public interface IPasswordHasher
{
    /// <summary>
    ///     Получить hash пароля
    /// </summary>
    string GetPasswordHash(string password);

    /// <summary>
    ///     Проверить валидность пароля
    /// </summary>
    bool IsPasswordValid(string password, string passwordHash);
}