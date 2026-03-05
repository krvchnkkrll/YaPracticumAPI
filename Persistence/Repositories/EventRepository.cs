using Domain.Events;
using Domain.Events.Parameters;
using Persistence.Contracts.Repositories;

namespace Persistence.Repositories;

internal class EventRepository : IEventRepository
{
    private readonly List<Event> _events = [];

    /// <summary>
    ///     Заполняем лист
    /// </summary>
    public EventRepository()
    {
        var event1 = Event.Create(new CreateEventParameter
        {
            Title = "Событие 1",
            Description = null,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1).AddHours(1),
        });
        
        var event2 = Event.Create(new CreateEventParameter
        {
            Title = "Событие 2",
            Description = "Описание события 2",
            StartAt = DateTime.Now.AddDays(2),
            EndAt = DateTime.Now.AddDays(2).AddHours(2),
        });
        
        var event3 = Event.Create(new CreateEventParameter
        {
            Title = "Событие 3",
            Description = "Описание события 3",
            StartAt = DateTime.Now.AddDays(3),
            EndAt = DateTime.Now.AddDays(3).AddHours(3)
        });

        _events.AddRange([event1, event2, event3]);
    }

    /// <summary>
    ///     Получить событие по id
    /// </summary>
    public Event GetById(Guid eventId)
    {
        var eventToReturn = _events.SingleOrDefault(e => e.Id == eventId);
        
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
        return _events.ToList();
    }

    /// <summary>
    ///     Добавить событие
    /// </summary>
    public Event Add(CreateEventParameter parameter)
    {
        var newEvent = Event.Create(parameter);
        
        _events.Add(newEvent);
        
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

        _events.Remove(eventToRemove);
    }
}