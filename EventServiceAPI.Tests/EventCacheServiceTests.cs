using Events.Application.Interfaces.Cache;
using Events.Application.Interfaces.Repositories;
using Events.Application.Models;
using Events.Application.Services;
using Events.Domain.Entities.Events;
using Events.Domain.Entities.Events.Parameters;
using FluentAssertions;
using Moq;

namespace EventServiceAPI.Tests;

public sealed class EventCacheServiceTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IEventCached> _eventCached = new();
    private readonly EventService _eventService;

    public EventCacheServiceTests()
    {
        _eventService = new EventService(_eventRepository.Object, _eventCached.Object);
    }

    [Fact]
    public async Task GetEventByIdAsync_CacheHit_NotCallRepository()
    {
        var cachedEvent = CreateEvent("Событие из кеша", 10);

        _eventCached
            .Setup(c => c.GetCachedEventByIdAsync(cachedEvent.Id))
            .ReturnsAsync(cachedEvent);

        var result = await _eventService.GetEventByIdAsync(cachedEvent.Id, CancellationToken.None);

        result.Id.Should().Be(cachedEvent.Id);
        result.Title.Should().Be(cachedEvent.Title);

        _eventRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));

        _eventCached.Verify(c => c.CreateCacheEventAsync(It.IsAny<Event>()));
    }

    [Fact]
    public async Task GetEventByIdAsync_CacheMiss_ReadFromRepositoryAndWarmCache()
    {
        var eventEntity = CreateEvent("Событие из базы", 10);

        _eventCached.Setup(cached => cached.GetCachedEventByIdAsync(eventEntity.Id)).ReturnsAsync((Event?)null);

        _eventRepository.Setup(repository => repository.GetByIdAsync(eventEntity.Id, CancellationToken.None)).ReturnsAsync(eventEntity);

        var result = await _eventService.GetEventByIdAsync(eventEntity.Id, CancellationToken.None);

        result.Id.Should().Be(eventEntity.Id);

        _eventRepository.Verify(r => r.GetByIdAsync(eventEntity.Id, CancellationToken.None));

        _eventCached.Verify(c => c.CreateCacheEventAsync(eventEntity));
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheHit_NotCallRepository()
    {
        var cachedEvents = new[]
        {
            CreateEvent("Топ 1", 10),
            CreateEvent("Топ 2", 20)
        };

        _eventCached.Setup(cached => cached.GetCachedTopEventsAsync()).ReturnsAsync(cachedEvents);

        var result = await _eventService.GetTopEventsAsync(CancellationToken.None);

        result.Select(response => response.Id).Should().BeEquivalentTo(cachedEvents.Select(@event => @event.Id));

        _eventRepository.Verify(repository => repository.GetTopEventsAsync(CancellationToken.None));

        _eventCached.Verify(cached => cached.CreateTopCacheEventsAsync(It.IsAny<IReadOnlyList<Event>>()));
    }

    [Fact]
    public async Task GetTopEventsAsync_CacheMiss_ReadFromRepositoryAndWarmCache()
    {
        var topEvents = new List<Event>
        {
            CreateEvent("Топ 1", 10), 
            CreateEvent("Топ 2", 20)
        };

        _eventCached.Setup(cached => cached.GetCachedTopEventsAsync()).ReturnsAsync([]);

        _eventRepository.Setup(repository => repository.GetTopEventsAsync(CancellationToken.None)).ReturnsAsync(topEvents);

        var result = await _eventService.GetTopEventsAsync(CancellationToken.None);

        result.Select(response => response.Id).Should().BeEquivalentTo(topEvents.Select(@event => @event .Id));

        _eventRepository.Verify(repository => repository.GetTopEventsAsync(CancellationToken.None));

        _eventCached.Verify(cached => cached.CreateTopCacheEventsAsync(topEvents));
    }

    [Fact]
    public async Task UpdateEventAsync_AfterSuccessfulUpdate_InvalidatesEventCache()
    {
        var eventEntity = CreateEvent("Событие до обновления", 10);

        _eventRepository.Setup(repository => repository.GetByIdAsync(eventEntity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);

        var request = new UpdateEventRequest
        {
            Title = "Обновлённое событие",
            Description = "Обновлённое описание",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        await _eventService.UpdateEventAsync(eventEntity.Id, request, CancellationToken.None);

        _eventRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()));

        _eventCached.Verify(cached => cached.RemoveCachedEventAsync(eventEntity.Id));
    }

    [Fact]
    public async Task DeleteEventAsync_AfterSuccessfulDelete_InvalidatesEventCache()
    {
        var eventEntity = CreateEvent("Событие на удаление", 10);

        _eventRepository.Setup(repository => repository.GetByIdAsync(eventEntity.Id, CancellationToken.None)).ReturnsAsync(eventEntity);

        await _eventService.DeleteEventAsync(eventEntity.Id, CancellationToken.None);

        _eventRepository.Verify(repository => repository.Remove(eventEntity));
        _eventRepository.Verify(repository => repository.SaveChangesAsync(CancellationToken.None));

        _eventCached.Verify(cached => cached.RemoveCachedEventAsync(eventEntity.Id));
    }

    private static Event CreateEvent(string title, int totalSeats)
    {
        var startAt = DateTime.UtcNow.Date.AddDays(1).AddHours(1);

        return Event.Create(new CreateEventParameter
        {
            Title = title,
            Description = null,
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            TotalSeats = totalSeats
        });
    }
}
