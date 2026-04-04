using Application.Contracts.Models;
using Application.Contracts.Services;
using Domain.Events;
using Domain.Events.Parameters;
using Domain.Models.Pagination;
using Persistence.Contracts.Repositories;

namespace Application.Services;

internal sealed class EventService(IEventRepository eventRepository) : IEventService
{
    public PaginatedResult<GetEventResponse> GetPaginatedEvents(GetEventsSearchQuery searchQuery, PaginationQuery paginationQuery)
    {
        var paginatedEvents = eventRepository.GetPaginatedEvents(searchQuery, paginationQuery);

        return new PaginatedResult<GetEventResponse>
        {
            Items = paginatedEvents.Items.Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            }).ToArray(),
            TotalItems = paginatedEvents.TotalItems,
            CurrentPage = paginatedEvents.CurrentPage,
            TotalPage = paginatedEvents.TotalPage,
            PageSize = paginatedEvents.PageSize,
            TotalPages = paginatedEvents.TotalPages
        };
    }

    public IList<Event> GetEvents()
    {
        return eventRepository.GetAllEvents();
    }

    public GetEventResponse GetEvent(Guid eventId)
    {
        var eventToReturn = eventRepository.GetById(eventId);
        
        return new GetEventResponse
        {
            Id = eventToReturn.Id,
            Title = eventToReturn.Title,
            Description = eventToReturn.Description,
            StartAt = eventToReturn.StartAt,
            EndAt = eventToReturn.EndAt
        };
    }
    
    public CreateEventResponse CreateEvent(CreateEventRequest request)
    {
        var newEvent = eventRepository.Add(new CreateEventParameter
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
        });

        return new CreateEventResponse
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt
        };
    }

    public void UpdateEvent(Guid eventId, UpdateEventRequest request)
    {
        eventRepository.Update(eventId, new UpdateEventParameter
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
        });
    }

    public void DeleteEvent(Guid eventId)
    {
        eventRepository.Delete(eventId);
    }
}