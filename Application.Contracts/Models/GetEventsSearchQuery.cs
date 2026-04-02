namespace Application.Contracts.Models;

public sealed class GetEventsSearchQuery
{
    /// <summary>
    ///     Название события
    /// </summary>
    public required string? Title { get; init; }
    
    /// <summary>
    ///     Начинаются с
    /// </summary>
    public required DateTime? From { get; init; }
    
    /// <summary>
    ///     Заканчиваются не позднее
    /// </summary>
    public required DateTime? To { get; init; }
}