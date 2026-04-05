using Application.Contracts.Models;
using Application.Services;
using Domain.Events;
using Domain.Events.Parameters;
using Domain.Models.Pagination;
using FluentAssertions;
using Moq;
using Persistence.Contracts.Repositories;
using Persistence.Contracts.Storages;
using Persistence.Repositories;

namespace Tests;

public sealed class EventServiceTests
{
    private Mock<IEventRepository> MockRepository { get; } = new();
    private static readonly Guid TestId = Guid.Parse("3f2504e0-4f89-41d3-9a0c-0305e82c3301");

    private readonly IList<Event> _events =
    [
        Event.Create(new CreateEventParameter
        {
            Id = TestId,
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
    public void Create_NewEvent_ReturnCreatedEvent()
    {
        MockRepository
            .Setup(r => r.Add(It.IsAny<CreateEventParameter>()))
            .Returns((CreateEventParameter p) => Event.Create(p));

        var service = new EventService(MockRepository.Object);
        var startAt = new DateTime();
        var endAt = startAt.AddDays(1);
        var request = new CreateEventRequest
        {
            Title = "Coбытие 4",
            Description = "Описание события 4",
            StartAt = startAt,
            EndAt = endAt
        };

        var result = service.CreateEvent(request);

        MockRepository.Verify(r => r.Add(It.Is<CreateEventParameter>(p =>
            p.Title == request.Title &&
            p.Description == request.Description &&
            p.StartAt == request.StartAt &&
            p.EndAt == request.EndAt)));
        
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
    }

    [Fact]
    public void Get_AllEvents_ReturnAllEvents()
    {
        MockRepository
            .Setup(r => r.GetAllEvents())
            .Returns(() => _events);

        var service = new EventService(MockRepository.Object);

        var result = service.GetEvents();

        result.Should().BeEquivalentTo(_events, opts => opts.WithoutStrictOrdering());
    }

    [Fact]
    public void Get_EventById_ReturnEvent()
    {
        MockRepository
            .Setup(r => r.GetById(TestId))
            .Returns(() => _events.Single(e => e.Id == TestId));

        var service = new EventService(MockRepository.Object);

        var result = service.GetEvent(TestId);
        var expectedEvent = _events.Select(e => new GetEventResponse
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt
        }).Single(e => e.Id == TestId);

        result.Should().BeEquivalentTo(expectedEvent);
    }

    [Fact]
    public void Update_EventById_WithOutExceptions()
    {
        var startAt = new DateTime();
        var endAt = startAt.AddDays(1);
        var request = new UpdateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = startAt,
            EndAt = endAt
        };
        var expectedParameter = new UpdateEventParameter
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
        };
        MockRepository
            .Setup(r => r.Update(TestId, It.IsAny<UpdateEventParameter>()));

        var service = new EventService(MockRepository.Object);

        service.UpdateEvent(TestId, request);

        MockRepository.Verify(r => r.Update(TestId, expectedParameter), Times.Once);
    }

    [Fact]
    public void Delete_EventBy_WithOutExceptions()
    {
        var deletedEvent = _events.Single(e => e.Id == TestId);

        MockRepository
            .Setup(r => r.Delete(TestId));

        var service = new EventService(MockRepository.Object);

        FluentActions.Invoking(() => service.DeleteEvent(deletedEvent.Id))
            .Should()
            .NotThrow();

        MockRepository.Verify(r => r.Delete(TestId));
    }

    [Fact]
    public void Get_FilteredByTitle_ReturnFilteredEvents()
    {
        var paginationQuery = new PaginationQuery
        {
            Page = 1,
            PageSize = 10
        };

        var searchQuery = new GetEventsSearchQuery
        {
            Title = "БЫТИЕ 3",
            From = null,
            To = null
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());

        var repository = new EventRepository(mockStorage.Object);
        var service = new EventService(repository);

        var result = service.GetPaginatedEvents(searchQuery, paginationQuery);

        var expectedEvents = _events
            .Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            }).Where(e => e.Title == "Событие 3").ToList();

        var expectedResult = new PaginatedResult<GetEventResponse>
        {
            Items = expectedEvents,
            TotalItems = expectedEvents.Count,
            CurrentPage = paginationQuery.Page,
            PageSize = paginationQuery.PageSize,
            TotalPages = expectedEvents.Count == 0 
                ? 0 
                : (int)Math.Ceiling(expectedEvents.Count / (double)paginationQuery.PageSize),
        };
        
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Get_FilteredByFromDateAndToDate_ReturnFilteredEvents()
    {
        var paginationQuery = new PaginationQuery
        {
            Page = 1,
            PageSize = 10
        };

        var searchQuery = new GetEventsSearchQuery
        {
            Title = null,
            From = DateTime.Now.AddDays(1),
            To = DateTime.Now.AddDays(2).AddHours(2)
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());

        var repository = new EventRepository(mockStorage.Object);
        var service = new EventService(repository);

        var result = service.GetPaginatedEvents(searchQuery, paginationQuery);

        var expectedEvents = _events
            .Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            })
            .Where(e =>
                e.StartAt >= searchQuery.From &&
                e.EndAt <= searchQuery.To)
            .ToList();

        var expectedResult = new PaginatedResult<GetEventResponse>
        {
            Items = expectedEvents,
            TotalItems = expectedEvents.Count,
            CurrentPage = paginationQuery.Page,
            PageSize = paginationQuery.PageSize,
            TotalPages = expectedEvents.Count == 0 
                ? 0 
                : (int)Math.Ceiling(expectedEvents.Count / (double)paginationQuery.PageSize),
        };
        
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void Get_CombinedFilterEvents_ReturnFilteredEvents()
    {
        var paginationQuery = new PaginationQuery
        {
            Page = 1,
            PageSize = 3
        };

        var searchQuery = new GetEventsSearchQuery
        {
            Title = "Событие 7",
            From = DateTime.Now.AddDays(7),
            To = DateTime.Now.AddDays(7).AddHours(7),
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());

        var repository = new EventRepository(mockStorage.Object);
        var service = new EventService(repository);

        var result = service.GetPaginatedEvents(searchQuery, paginationQuery);

        var orderedResponses = _events
            .Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            })
            .Where(e =>
                e.StartAt >= searchQuery.From &&
                e.EndAt <= searchQuery.To &&
                e.Title.Contains(searchQuery.Title, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(e => e.Id)
            .ToList();

        var totalItems = orderedResponses.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)paginationQuery.PageSize);

        var skip = (paginationQuery.Page - 1) * paginationQuery.PageSize;
        var expectedPageItems = orderedResponses
            .Skip(skip)
            .Take(paginationQuery.PageSize)
            .ToList();

        var expectedResult = new PaginatedResult<GetEventResponse>
        {
            Items = expectedPageItems,
            TotalItems = totalItems,
            CurrentPage = paginationQuery.Page,
            PageSize = paginationQuery.PageSize,
            TotalPages = totalPages
        };

        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void Get_PaginatedEvents_ReturnPaginatedEvents()
    {
        var paginationQuery = new PaginationQuery
        {
            Page = 2,
            PageSize = 2
        };

        var searchQuery = new GetEventsSearchQuery
        {
            Title = null,
            From = null,
            To = null
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());

        var repository = new EventRepository(mockStorage.Object);
        var service = new EventService(repository);

        var result = service.GetPaginatedEvents(searchQuery, paginationQuery);

        var orderedResponses = _events
            .Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            })
            .OrderBy(e => e.Id)
            .ToList();

        var totalItems = orderedResponses.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)paginationQuery.PageSize);

        var skip = (paginationQuery.Page - 1) * paginationQuery.PageSize;
        var expectedPageItems = orderedResponses
            .Skip(skip)
            .Take(paginationQuery.PageSize)
            .ToList();

        var expectedResult = new PaginatedResult<GetEventResponse>
        {
            Items = expectedPageItems,
            TotalItems = totalItems,
            CurrentPage = paginationQuery.Page,
            PageSize = paginationQuery.PageSize,
            TotalPages = totalPages
        };

        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void Get_EventByNotExistId_Throw404NotFoundException()
    {
        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
         
        var repository = new EventRepository(mockStorage.Object);
        var service = new EventService(repository);

        FluentActions
            .Invoking(() => service.GetEvent(Guid.CreateVersion7()))
            .Should()
            .Throw<KeyNotFoundException>();
    }
    
    [Fact]
    public void Update_EventByNotExistId_ThrowKeyNotFoundException()
    {
        var request = new UpdateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.Now.AddDays(4),
            EndAt = DateTime.Now.AddDays(4).AddHours(4),
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(mockStorage.Object);
        
        var service = new EventService(repository);
        
        FluentActions
            .Invoking(() => service.UpdateEvent(Guid.CreateVersion7(), request))
            .Should()
            .Throw<KeyNotFoundException>();
    }
    
    [Fact]
    public void Create_EventWithIncorrectData_ArgumentException()
    {
        var request = new CreateEventRequest
        {
            Title = "",
            Description = "",
            StartAt = DateTime.Now.AddDays(4),
            EndAt = DateTime.Now.AddDays(4).AddHours(4),
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(mockStorage.Object);
        
        var service = new EventService(repository);
        
        FluentActions
            .Invoking(() => service.CreateEvent(request))
            .Should()
            .Throw<ArgumentException>();
    }
    
    [Fact]
    public void Update_EventWithIncorrectData_ArgumentException()
    {
        var request = new UpdateEventRequest
        {
            Title = "Событие 4",
            Description = "Описание события 4",
            StartAt = DateTime.Now.AddDays(4).AddHours(4),
            EndAt = DateTime.Now.AddDays(4),
        };

        var mockStorage = new Mock<IEventStorage>();
        mockStorage
            .Setup(s => s.Events)
            .Returns(() => _events.ToList());
        
        var repository = new EventRepository(mockStorage.Object);
        
        var service = new EventService(repository);
        
        FluentActions
            .Invoking(() => service.UpdateEvent(TestId, request))
            .Should()
            .Throw<ArgumentException>();
    }
}