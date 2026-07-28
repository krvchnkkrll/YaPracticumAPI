using Events.Domain.Entities.Events;

namespace Events.Application.Interfaces.Cache;

public interface IEventCached
{
    /// <summary>
    ///     Получить топ событий
    /// </summary>
    Task<Event[]> GetCachedTopEventsAsync();

    /// <summary>
    ///     Получить событие
    /// </summary>
    Task<Event?> GetCachedEventByIdAsync(Guid eventId);

    /// <summary>
    ///     Записать событие
    /// </summary>
    Task CreateCacheEventAsync(Event @event);
    
    /// <summary>
    ///     Записать события
    /// </summary>
    Task CreateTopCacheEventsAsync(IReadOnlyList<Event> events);

    /// <summary>
    ///     Инвалидировать кеш события
    /// </summary>
    Task RemoveCachedEventAsync(Guid eventId);
}