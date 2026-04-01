using Application.Contracts.Models.GetEvents;
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
    public IEnumerable<Event> GetAllEvents(GetEventsSearchQuery searchQuery)
    {
        var query = eventStorage.Events.AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery.Title))
        {
            query = query.Where(e => e.Title.Contains(searchQuery.Title, StringComparison.InvariantCultureIgnoreCase));
        }

        if (!searchQuery.From.HasValue)
        {
            query = query.Where(e => e.StartAt >= searchQuery.From);
        }

        if (!searchQuery.To.HasValue)
        {
            query = query.Where(e => e.EndAt <= searchQuery.To);
        }
        
        return query.ToList();
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
    public void Update(Guid eventId, UpdateEventParameter parameter)
    {
        var eventToUpdate = GetById(eventId);
        
        eventToUpdate.Update(parameter);
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