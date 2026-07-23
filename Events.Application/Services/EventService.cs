using System.ComponentModel.DataAnnotations;
using Events.Application.Interfaces.Repositories;
using Events.Application.Interfaces.Services;
using Events.Application.Models;
using Events.Domain.Entities.Events;
using Events.Domain.Entities.Events.Parameters;
using Events.Domain.Models.Pagination;

namespace Events.Application.Services;

public sealed class EventService(IEventRepository eventRepository) : IEventService
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
            Items = paginatedEvents.Items.Select(e => new GetEventResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt,
                TotalSeats = e.TotalSeats,
                AvailableSeats = e.AvailableSeats
            }).ToArray(),
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

    public async Task<GetEventResponse> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetReadOnlyByIdAsync(eventId, cancellationToken);
        
        return new GetEventResponse
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
    }
}