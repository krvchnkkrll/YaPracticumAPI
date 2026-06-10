using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Persistence.Contracts.Storages;

namespace Persistence.Storages;

internal sealed class EventStorage : IEventStorage
{
    public List<Event> Events { get; } = [];

    public EventStorage()
    {
        var event1 = Event.Create(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "Событие 1",
            Description = null,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1)
                .AddHours(1),
            TotalSeats = 5,
        });
        
        var event2 = Event.Create(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "Событие 2",
            Description = "Описание события 2",
            StartAt = DateTime.Now.AddDays(2),
            EndAt = DateTime.Now.AddDays(2)
                .AddHours(2),
            TotalSeats = 9,
        });
        
        var event3 = Event.Create(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "Событие 3",
            Description = "Описание события 3",
            StartAt = DateTime.Now.AddDays(3),
            EndAt = DateTime.Now.AddDays(3)
                .AddHours(3),
            TotalSeats = 13,
        });
        
        Events.AddRange([event1, event2, event3]);
    }
}