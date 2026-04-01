namespace Application.Contracts.Models.GetEvents;

public sealed class GetEventsSearchQuery
{
    public required string? Title { get; init; }
    public required DateTime? From { get; init; }
    public required DateTime? To { get; init; }
}