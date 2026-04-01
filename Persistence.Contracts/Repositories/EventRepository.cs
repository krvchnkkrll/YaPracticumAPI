using Domain.Events;
using Domain.Events.Parameters;

namespace Persistence.Contracts.Repositories;

public interface IEventRepository
{
    /// <summary>
    ///     Получить событие по id
    /// </summary>
    Event GetById(Guid eventId);

    /// <summary>
    ///     Получить все события
    /// </summary>
    IEnumerable<Event> GetAllEvents();

    /// <summary>
    ///     Добавить событие
    /// </summary>
    Event Add(CreateEventParameter parameter);

    /// <summary>
    ///     Обновить событие
    /// </summary>
    void Update(Guid eventId, UpdateEventParameter parameter);

    /// <summary>
    ///     Удалить событие
    /// </summary>
    void Delete(Guid eventId);
}