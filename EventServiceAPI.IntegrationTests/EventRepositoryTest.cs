using Events.Application.Models;
using Events.Domain.Entities.Events;
using Events.Domain.Entities.Events.Parameters;
using Events.Domain.Models.Pagination;
using EventServiceAPI.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EventServiceAPI.IntegrationTests;

public class EventRepositoryTest : IntegrationTestBase
{
    private readonly CreateEventParameter _eventParameter = new CreateEventParameter
    {
        Title = "Test1",
        Description = "ForIntegrationsTests",
        StartAt = DateTime.UtcNow.AddHours(1).TruncateToMicroseconds(),
        EndAt = DateTime.UtcNow.AddDays(1).TruncateToMicroseconds(),
        TotalSeats = 10
    };

    [Fact]
    public async Task GetReadOnlyEventByIdAsync_WithNoTracking_ReturnReadOnlyEvent()
    {
        var eventEntity = await CreateAndSaveEventAsync();

        var retrievedEventEntity = await EventRepository.GetReadOnlyByIdAsync(eventEntity.Id, CancellationToken.None);
        retrievedEventEntity.Update(new UpdateEventParameter
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = eventEntity.StartAt,
            EndAt = eventEntity.EndAt,
        });

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var retrievedEventEntityAfterUpdate =
            await EventRepository.GetReadOnlyByIdAsync(eventEntity.Id, CancellationToken.None);

        retrievedEventEntityAfterUpdate.Should().BeEquivalentTo(eventEntity);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithTracking_ReturnEvent()
    {
        var eventEntity = await CreateAndSaveEventAsync();

        var retrievedEventEntity = await EventRepository.GetByIdAsync(eventEntity.Id, CancellationToken.None);
        retrievedEventEntity.Update(new UpdateEventParameter
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = eventEntity.StartAt,
            EndAt = eventEntity.EndAt,
        });

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var retrievedEventEntityAfterUpdate =
            await EventRepository.GetReadOnlyByIdAsync(eventEntity.Id, CancellationToken.None);

        retrievedEventEntityAfterUpdate.Should().BeEquivalentTo(retrievedEventEntity);
    }

    [Fact]
    public async Task GetAllReadOnlyEvents_WithNoTracking_ReturnsAllReadOnlyEvents()
    {
        var firstEventEntity = await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();

        var events = await EventRepository.GetAllReadOnlyAsync(CancellationToken.None);

        var eventEntity = events[0];
        eventEntity.Update(new UpdateEventParameter
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = firstEventEntity.StartAt,
            EndAt = firstEventEntity.EndAt,
        });

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var eventEntityAfterUpdate =
            await EventRepository.GetReadOnlyByIdAsync(firstEventEntity.Id, CancellationToken.None);
        eventEntityAfterUpdate.Should().BeEquivalentTo(firstEventEntity);
    }

    [Fact]
    public async Task GetAllReadOnlyEvents_WithTracking_ReturnsAllEvents()
    {
        var firstEventEntity = await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();

        var events = await EventRepository.GetAllAsync(CancellationToken.None);

        var eventEntity = events[0];
        eventEntity.Update(new UpdateEventParameter
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = firstEventEntity.StartAt,
            EndAt = firstEventEntity.EndAt,
        });

        await EventRepository.SaveChangesAsync(CancellationToken.None);

        var eventEntityAfterUpdate =
            await EventRepository.GetReadOnlyByIdAsync(firstEventEntity.Id, CancellationToken.None);
        eventEntityAfterUpdate.Should().BeEquivalentTo(eventEntity);
    }

    [Fact]
    public async Task Remove_ExistingEvent_DeletesFromDatabase()
    {
        var eventEntity = await CreateAndSaveEventAsync();

        EventRepository.Remove(eventEntity);
        await EventRepository.SaveChangesAsync(CancellationToken.None);

        await FluentActions
            .Invoking(() => EventRepository.GetByIdAsync(eventEntity.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetPaginatedAsync_WithoutFilters_ReturnsPaginatedEvents()
    {
        await CreateEventCollectionAsync();

        var result = await EventRepository.GetPaginatedAsync(
            new GetEventsSearchQuery(),
            new PaginationQuery { Page = 1, PageSize = 2 },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalItems.Should().BeGreaterThanOrEqualTo(7);
        result.TotalPages.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetPaginatedAsync_Pagination_ReturnsCorrectPages()
    {
        await CreateEventCollectionAsync();

        var page1 = await EventRepository.GetPaginatedAsync(
            new GetEventsSearchQuery(),
            new PaginationQuery { Page = 1, PageSize = 3 },
            CancellationToken.None);

        var page2 = await EventRepository.GetPaginatedAsync(
            new GetEventsSearchQuery(),
            new PaginationQuery { Page = 2, PageSize = 3 },
            CancellationToken.None);

        var page3 = await EventRepository.GetPaginatedAsync(
            new GetEventsSearchQuery(),
            new PaginationQuery { Page = 3, PageSize = 3 },
            CancellationToken.None);

        page1.Items.Should().HaveCount(3);
        page2.Items.Should().HaveCount(3);
        page3.Items.Should().HaveCount(1);

        var page1Ids = page1.Items.Select(e => e.Id).ToList();
        var page2Ids = page2.Items.Select(e => e.Id).ToList();
        var page3Ids = page3.Items.Select(e => e.Id).ToList();

        page1Ids.Should().NotIntersectWith(page2Ids);
        page2Ids.Should().NotIntersectWith(page3Ids);
        page1Ids.Should().NotIntersectWith(page3Ids);
    }

    private async Task<Event> CreateAndSaveEventAsync()
    {
        var eventEntity = Event.Create(_eventParameter);
        EventRepository.Add(eventEntity);
        await EventRepository.SaveChangesAsync(CancellationToken.None);
        return eventEntity;
    }

    private async Task CreateEventCollectionAsync()
    {
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
        await CreateAndSaveEventAsync();
    }
}