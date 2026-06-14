using Application.Contracts.Models;
using Domain.Entities.Events;
using Domain.Models.Pagination;

namespace Application.Contracts.Services;

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
    Task UpdateEventAsync(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Удалить событие
    /// </summary>
    Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken);
}