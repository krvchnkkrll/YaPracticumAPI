using Application.Contracts.Models;
using Domain.Events;
using Domain.Events.Parameters;
using Domain.Models.Pagination;

namespace Persistence.Contracts.Repositories;

public interface IEventRepository
{
    /// <summary>
    ///     Получить событие по id
    /// </summary>
    Event GetById(Guid eventId);

    /// <summary>
    ///     Получить пагинируемые события
    /// </summary>
    PaginatedResult<Event> GetPaginatedEvents(GetEventsSearchQuery searchQuery, PaginationQuery paginationQuery);

    /// <summary>
    ///     Получить события
    /// </summary>
    /// <returns></returns>
    IList<Event> GetAllEvents();

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