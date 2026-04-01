using Application.Contracts.Models;
using Application.Contracts.Models.GetEvents;

namespace Application.Contracts.Services;

public interface IEventService
{
    /// <summary>
    ///     Получить все события
    /// </summary>
    GetEventsResponse GetEvents(GetEventsSearchQuery searchQuery);
    
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