using Events.Application.Models;
using Events.Domain.Entities.Events;
using Events.Domain.Models.Pagination;

namespace Events.Application.Interfaces.Services;

public interface IEventService
{
    /// <summary>
    ///     Получить пагинируемые события
    /// </summary>
    Task<PaginatedResult<GetEventResponse>> GetPaginatedAsync(GetEventsSearchQuery searchQuery, 
        PaginationQuery paginationQuery, CancellationToken cancellationToken);

    /// <summary>
    ///     Получить все события
    /// </summary>
    Task<IReadOnlyList<Event>> GetEventsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить топ-10 событий по проценту проданных мест
    /// </summary>
    Task<IEnumerable<GetEventResponse>> GetTopEventsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Получить событие
    /// </summary>
    Task<GetEventResponse> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken);
    
    /// <summary>
    ///     Создать событие
    /// </summary>
    Task<CreateEventResponse> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Обновить событие
    /// </summary>
    Task<UpdateEventResponse> UpdateEventAsync(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Удалить событие
    /// </summary>
    Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken);
}