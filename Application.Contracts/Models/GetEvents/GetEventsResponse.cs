namespace Application.Contracts.Models.GetEvents;

public sealed class GetEventsResponse
{
    public required IEnumerable<GetEventResponse> Events { get; init; }
}