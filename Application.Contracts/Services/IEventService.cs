using Application.Contracts.Models;
using Domain.Events;

namespace Application.Contracts.Services;

public interface IEventService
{
    /// <summary>
    ///     Получить все события
    /// </summary>
    IEnumerable<GetEventResponse> GetEvents();
    
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
    UpdateEventResponse UpdateEvent(Guid eventId, UpdateEventRequest request);
    
    /// <summary>
    ///     Удалить событие
    /// </summary>
    void DeleteEvent(Guid eventId);
}