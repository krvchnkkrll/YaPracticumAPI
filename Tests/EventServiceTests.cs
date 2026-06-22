using System.ComponentModel.DataAnnotations;
using Application.Contracts.Models;
using Application.Contracts.Services;
using Application.Services;
using Domain.Entities.Events;
using Domain.Entities.Events.Parameters;
using Domain.Models.Pagination;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Contracts;
using Persistence.Contracts.Repositories;
using Persistence.Repositories;

namespace Tests;

public sealed class EventServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    private static readonly List<Event> Events =
    [
        CreateEvent("Событие 1", null, 1, 5),
        CreateEvent("Событие 2", "Описание события 2", 2, 9),
        CreateEvent("Событие 3", "Описание события 3", 3, 13)
    ];

    public EventServiceTests()
    {
        var dbName = Guid.CreateVersion7().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventService, EventService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Create_NewEvent_ReturnCreatedEvent()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var request = new CreateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.UtcNow.AddDays(4),
            EndAt = DateTime.UtcNow.AddDays(4).AddHours(1),
            TotalSeats = 20
        };

        var result = await service.CreateEventAsync(request, CancellationToken.None);

        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
        Assert.Equal(request.TotalSeats, result.TotalSeats);
        // При создании мероприятия количество доступных мест приравнивается кол-ву мест на событии
        Assert.Equal(request.TotalSeats, result.AvailableSeats);
    }

    [Fact]
    public async Task Get_AllEvents_ReturnAllEvents()
    {
        await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetEventsAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(Events, opts => opts.WithoutStrictOrdering());
    }

    [Fact]
    public async Task Get_EventById_ReturnEvent()
    {
        var events = await SeedEventsAsync();
        var expected = events[0];

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetEventByIdAsync(expected.Id, CancellationToken.None);

        result.Should().BeEquivalentTo(new GetEventResponse
        {
            Id = expected.Id,
            Title = expected.Title,
            Description = expected.Description,
            StartAt = expected.StartAt,
            EndAt = expected.EndAt,
            TotalSeats = expected.TotalSeats,
            AvailableSeats = expected.AvailableSeats
        });
    }

    [Fact]
    public async Task Update_EventById_WithOutExceptions()
    {
        var events = await SeedEventsAsync();
        var eventId = events[0].Id;

        var request = new UpdateEventRequest
        {
            Title = "Обновлённое событие",
            Description = "Обновлённое описание",
            StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(2)
        };

        using var scope = _serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
        await service.UpdateEventAsync(eventId, request, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await service.GetEventByIdAsync(eventId, CancellationToken.None);

        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
    }

    [Fact]
    public async Task Delete_EventById_WithOutExceptions()
    {
        var events = await SeedEventsAsync();
        var eventId = events[0].Id;

        using var scope = _serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IEventService>();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
        
        await service.DeleteEventAsync(eventId, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        await FluentActions
            .Invoking(() => service.GetEventByIdAsync(eventId, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Get_FilteredByTitle_ReturnFilteredEvents()
    {
        await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetPaginatedAsync(
            new GetEventsSearchQuery
            {
                Title = "БЫТИЕ 3",
                From = null,
                To = null
            },
            new PaginationQuery
            {
                Page = 1,
                PageSize = 10
            },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items.Single().Title.Should().Be("Событие 3");
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Get_FilteredByFromDateAndToDate_ReturnFilteredEvents()
    {
        var events = await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetPaginatedAsync(
            new GetEventsSearchQuery
            {
                Title = null,
                From = events[0].StartAt,
                To = events[1].EndAt
            },
            new PaginationQuery
            {
                Page = 1,
                PageSize = 10
            },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Select(e => e.Title).Should().BeEquivalentTo("Событие 1", "Событие 2");
    }

    [Fact]
    public async Task Get_CombinedFilterEvents_ReturnFilteredEvents()
    {
        var events = await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetPaginatedAsync(
            new GetEventsSearchQuery
            {
                Title = "Событие 2",
                From = events[1].StartAt,
                To = events[1].EndAt
            },
            new PaginationQuery
            {
                Page = 1,
                PageSize = 3
            },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items.Single().Title.Should().Be("Событие 2");
        result.TotalItems.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Get_PaginatedEvents_ReturnPaginatedEvents()
    {
        var events = await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var result = await service.GetPaginatedAsync(
            new GetEventsSearchQuery(),
            new PaginationQuery
            {
                Page = 2,
                PageSize = 2
            },
            CancellationToken.None);

        var expected = events
            .OrderBy(e => e.Id)
            .Skip(2)
            .Take(2)
            .Select(e => e.Id);

        result.Items.Select(e => e.Id)
            .Should()
            .BeEquivalentTo(expected, options => options.WithStrictOrdering());
        
        result.TotalItems.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Get_EventByNotExistId_ThrowsKeyNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        await FluentActions
            .Invoking(() => service.GetEventByIdAsync(Guid.CreateVersion7(), CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_EventByNotExistId_ThrowsKeyNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var request = new UpdateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.UtcNow.AddDays(4),
            EndAt = DateTime.UtcNow.AddDays(4).AddHours(4)
        };

        await FluentActions
            .Invoking(() => service.UpdateEventAsync(Guid.CreateVersion7(), request, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Create_EventWithIncorrectData_ThrowsArgumentException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var request = new CreateEventRequest
        {
            Title = "",
            Description = "",
            StartAt = DateTime.UtcNow.AddDays(4),
            EndAt = DateTime.UtcNow.AddDays(4).AddHours(4),
            TotalSeats = 20
        };

        await FluentActions
            .Invoking(() => service.CreateEventAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Create_EventWithoutTotalSeats_ThrowsValidationException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var request = new CreateEventRequest
        {
            Title = "Событие",
            Description = null,
            StartAt = DateTime.UtcNow.AddDays(4),
            EndAt = DateTime.UtcNow.AddDays(4).AddHours(4),
            TotalSeats = null
        };

        await FluentActions
            .Invoking(() => service.CreateEventAsync(request, CancellationToken.None))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Update_EventWithIncorrectData_ThrowsArgumentException()
    {
        var events = await SeedEventsAsync();

        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEventService>();

        var request = new UpdateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.UtcNow.AddDays(4).AddHours(4),
            EndAt = DateTime.UtcNow.AddDays(4)
        };

        await FluentActions
            .Invoking(() => service.UpdateEventAsync(events[0].Id, request, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>();
    }
    
    

    private async Task<List<Event>> SeedEventsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Events.AddRange(Events);
        await context.SaveChangesAsync(CancellationToken.None);
        var events = await context.Events.ToListAsync();

        return events;
    }

    private static Event CreateEvent(string title, string? description, int dayOffset, int totalSeats)
    {
        var startAt = DateTime.UtcNow.Date.AddDays(dayOffset).AddHours(10);

        return Event.Create(new CreateEventParameter
        {
            Title = title,
            Description = description,
            StartAt = startAt,
            EndAt = startAt.AddHours(dayOffset),
            TotalSeats = totalSeats
        });
    }
}