namespace Application.Models;

public sealed class GetEventsSearchQuery
{
    /// <summary>
    ///     Название события
    /// </summary>
    public string? Title { get; init; }
    
    /// <summary>
    ///     Начинаются с
    /// </summary>
    public DateTime? From { get; init; }
    
    /// <summary>
    ///     Заканчиваются не позднее
    /// </summary>
    public DateTime? To { get; init; }
}