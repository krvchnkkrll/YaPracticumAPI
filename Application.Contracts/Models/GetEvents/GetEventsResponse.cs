namespace Application.Contracts.Models.GetEvents;

public sealed class GetEventsResponse
{
    /// <summary>
    ///     Список событий
    /// </summary>
    public required IEnumerable<GetEventResponse> Events { get; init; }
}