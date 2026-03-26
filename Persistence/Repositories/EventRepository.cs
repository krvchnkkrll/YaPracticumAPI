using Domain.Events;
using Domain.Events.Parameters;
using Persistence.Contracts.Repositories;

namespace Persistence.Repositories;

internal class EventRepository(EventStorage eventStorage) : IEventRepository
{
    /// <summary>
    ///     Получить событие по id
    /// </summary>
    public Event GetById(Guid eventId)
    {
        var eventToReturn = eventStorage.Events.SingleOrDefault(e => e.Id == eventId);
        
        if (ReferenceEquals(eventToReturn, null))
            throw new KeyNotFoundException($"Событие с идентификатором {eventId} не найдено.");
        
        return eventToReturn;
    }

    /// <summary>
    ///     Получить все события
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Event> GetAllEvents()
    {
        return eventStorage.Events.ToList();
    }

    /// <summary>
    ///     Добавить событие
    /// </summary>
    public Event Add(CreateEventParameter parameter)
    {
        var newEvent = Event.Create(parameter);
        
        eventStorage.Events.Add(newEvent);
        
        return newEvent;
    }

    /// <summary>
    ///     Обновить событие
    /// </summary>
    public Event Update(Guid eventId, UpdateEventParameter parameter)
    {
        var eventToUpdate = GetById(eventId);
        
        return eventToUpdate.Update(parameter);
    }

    /// <summary>
    ///     Удалить событие
    /// </summary>
    public void Delete(Guid eventId)
    {
        var eventToRemove = GetById(eventId);

        eventStorage.Events.Remove(eventToRemove);
    }
}