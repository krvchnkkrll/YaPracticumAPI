using Application.Contracts.Models;
using Domain.Events;
using Domain.Models.Pagination;

namespace Application.Contracts.Services;

public interface IEventService
{
    /// <summary>
    ///     Получить пагинируемые события
    /// </summary>
    PaginatedResult<GetEventResponse> GetPaginatedEvents(GetEventsSearchQuery searchQuery, PaginationQuery paginationQuery);
    
    /// <summary>
    ///     Получить все события
    /// </summary>
    IList<Event> GetEvents();
    
    /// <summary>
    ///     Получить событие
    /// </summary>
    GetEventResponse? GetEvent(Guid eventId);
    
    /// <summary>
    ///     Создать событие
    /// </summary>
    CreateEventResponse CreateEvent(CreateEventRequest request);
    
    /// <summary>
    ///     Обновить событие
    /// </summary>
    void UpdateEvent(Guid eventId, UpdateEventRequest request);
    
    /// <summary>
    ///     Удалить событие
    /// </summary>
    void DeleteEvent(Guid eventId);
}