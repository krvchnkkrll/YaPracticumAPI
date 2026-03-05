namespace Application.Contracts.Models;

public sealed class GetEventsResponse
{
    public required IEnumerable<GetEventResponse> Events { get; init; }
}