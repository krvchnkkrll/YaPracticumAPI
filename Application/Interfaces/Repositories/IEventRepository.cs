using Application.Models;
using Domain.Entities.Bookings;
using Domain.Entities.Events;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories;

public interface IEventRepository
{
    /// <summary>
    ///     Получить событие с отслеживанием изменений.
    /// </summary>
    Task<Event> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Получить событие без отслеживания изменений.
    /// </summary>
    Task<Event> GetReadOnlyByIdAsync(Guid eventId, CancellationToken cancellationToken);

    /// <summary>
    ///     Получить страницу событий без отслеживания изменений.
    /// </summary>
    Task<PaginatedResult<Event>> GetPaginatedAsync(GetEventsSearchQuery searchQuery, PaginationQuery paginationQuery,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Получить все события без отслеживания изменений.
    /// </summary>
    Task<IReadOnlyList<Event>> GetAllReadOnlyAsync(CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить все события с отслеживанием изменений. 
    /// </summary>
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Добавить бронь к мероприятию
    /// </summary>
    Booking CreateBooking(Event eventEntity, Guid userId);

    /// <summary>
    ///     Добавить новое событие в контекст.
    /// </summary>
    void Add(Event eventEntity);

    /// <summary>
    ///     Удалить событие из контекста.
    /// </summary>
    void Remove(Event eventEntity);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    
    Task<Event> GetByIdWithIncludeBookingsAsync(Guid eventId, CancellationToken cancellationToken);
}