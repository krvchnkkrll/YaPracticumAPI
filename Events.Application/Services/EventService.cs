using System.ComponentModel.DataAnnotations;
using Events.Application.Interfaces.Cache;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Services;
using Events.Application.Models;
using Events.Domain.Entities.Events;
using Events.Domain.Entities.Events.Parameters;
using Events.Domain.Models.Pagination;
using static Events.Application.Common.Mappers.EventMappers;

namespace Events.Application.Services;

public sealed class EventService(
    IEventRepository eventRepository,
    IEventCached eventCached) : IEventService
{
    private const int DefaultPageSize = 10;
    private const int DefaultPage = 1;
    
    public async Task<PaginatedResult<GetEventResponse>> GetPaginatedAsync(GetEventsSearchQuery searchQuery, 
        PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        if (paginationQuery.Page == 0)
            paginationQuery.Page = DefaultPage;
        
        if (paginationQuery.PageSize == 0)
            paginationQuery.PageSize = DefaultPageSize;
        
        var paginatedEvents = await eventRepository.GetPaginatedAsync(searchQuery, paginationQuery, cancellationToken);

        return new PaginatedResult<GetEventResponse>
        {
            Items = paginatedEvents.Items.Select(ToEventResponse).ToArray(),
            TotalItems = paginatedEvents.TotalItems,
            CurrentPage = paginatedEvents.CurrentPage,
            PageSize = paginatedEvents.PageSize,
            TotalPages = paginatedEvents.TotalPages
        };
    }

    public async Task<IReadOnlyList<Event>> GetEventsAsync(CancellationToken cancellationToken)
    {
        return await eventRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<GetEventResponse>> GetTopEventsAsync(CancellationToken cancellationToken)
    {
        var cachedEvents = await eventCached.GetCachedTopEventsAsync();
        
        if (cachedEvents.Length > 0)
            return cachedEvents.Select(ToEventResponse);

        var topEvents = await eventRepository.GetTopEventsAsync(cancellationToken);

        await eventCached.CreateTopCacheEventsAsync(topEvents);

        return topEvents.Select(ToEventResponse);
    }

    public async Task<GetEventResponse> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var cachedEvent = await eventCached.GetCachedEventByIdAsync(eventId);
        
        if (cachedEvent != null)
            return ToEventResponse(cachedEvent);
        
        var @event = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        
        await eventCached.CreateCacheEventAsync(@event);
        
        return ToEventResponse(@event);
    }
    
    public async Task<CreateEventResponse> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        switch (request.TotalSeats)
        {
            case null:
                throw new ValidationException("Общее количество мест обязательно.");
            case 0:
                throw new ValidationException("Общее количество мест должно быть больше нуля.");
        }

        var newEvent = Event.Create(new CreateEventParameter
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            TotalSeats = request.TotalSeats.Value
        });
        
        eventRepository.Add(newEvent);

        await eventRepository.SaveChangesAsync(cancellationToken);
        
        return new CreateEventResponse
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
            TotalSeats = newEvent.TotalSeats,
            AvailableSeats = newEvent.AvailableSeats
        };
    }

    public async Task<UpdateEventResponse> UpdateEventAsync(Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        
        eventEntity.Update(new UpdateEventParameter
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
        });
        
        await eventRepository.SaveChangesAsync(cancellationToken);

        await eventCached.RemoveCachedEventAsync(eventId);

        return new UpdateEventResponse
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            StartAt = eventEntity.StartAt,
            EndAt = eventEntity.EndAt,
            TotalSeats = eventEntity.TotalSeats,
            AvailableSeats = eventEntity.AvailableSeats
        };
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        
        eventRepository.Remove(eventEntity);
        
        await eventRepository.SaveChangesAsync(cancellationToken);
        
        await eventCached.RemoveCachedEventAsync(eventId);
    }
}