using Events.Application.Models;
using Events.Domain.Entities.Events;

namespace Events.Application.Common.Mappers;

public static class EventMappers
{
    public static GetEventResponse ToEventResponse(Event @event)
    {
        return new GetEventResponse
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats,
        };
    }
}