using Domain.Events;
using Domain.Events.Parameters;
using FluentAssertions;
using Moq;
using Persistence.Contracts.Storages;
using Persistence.Repositories;

namespace Tests;

public sealed class EventRepositoryTests
{
    private Mock<IEventStorage> MockStorage{ get; } = new();
    private static readonly Guid TestGuid = Guid.CreateVersion7();
    
    private readonly IList<Event> _events =
    [
        Event.Create(new CreateEventParameter
        {
            Id = TestGuid,
            Title = "Событие 1",
            Description = null,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1).AddHours(1),
        }),
        Event.Create(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "Событие 2",
            Description = "Описание события 2",
            StartAt = DateTime.Now.AddDays(2),
            EndAt = DateTime.Now.AddDays(2).AddHours(2),
        }),
        Event.Create(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "Событие 3",
            Description = "Описание события 3",
            StartAt = DateTime.Now.AddDays(3),
            EndAt = DateTime.Now.AddDays(3).AddHours(3),
        })
    ];

    [Fact]
    public void Get_EventByNotExistId_ThrowKeyNotFoundException()
    {
        MockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(MockStorage.Object);
        
        FluentActions
            .Invoking(() => repository.GetById(Guid.CreateVersion7()))
            .Should()
            .Throw<KeyNotFoundException>();
    }
    
    [Fact]
    public void Update_EventByNotExistId_ThrowKeyNotFoundException()
    {
        var request = new UpdateEventParameter
        {
            Title = "Событие 3",
            Description = "Описание события 4",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(3),
        };

        MockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(MockStorage.Object);
        
        FluentActions
            .Invoking(() => repository.Update(Guid.CreateVersion7(), request))
            .Should()
            .Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Create_EventWithIncorrectData_ThrowArgumentException()
    {
        var request = new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = "      ",
            Description = "    ",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(3),
        };

        MockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(MockStorage.Object);
        
        FluentActions
            .Invoking(() => repository.Add(request))
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Update_EventWithIncorrectDateTime_ThrowArgumentException()
    {
        var request = new UpdateEventParameter
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.Now.AddDays(3),
            EndAt = DateTime.Now
        };
        
        MockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(MockStorage.Object);
        
        FluentActions
            .Invoking(() => repository.Update(TestGuid, request))
            .Should()
            .Throw<ArgumentException>();
    }
}