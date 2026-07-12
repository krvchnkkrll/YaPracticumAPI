using Domain.Entities.Users;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    /// <summary>
    ///     Создать пользователя
    /// </summary>
    void Add(User user);
    
    /// <summary>
    ///     Получить пользователя по логину
    /// </summary>
    Task<User?> GetByLoginAsync(string login, CancellationToken token);

    /// <summary>
    ///     Получить пользователя по логину
    /// </summary>
    Task<User> GetReadOnlyByIdAsync(Guid userId, CancellationToken token);

    Task SaveChangesAsync(CancellationToken token);
}