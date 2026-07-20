namespace Events.Application.Models;

public sealed class CreateEventRequest
{
    /// <summary>
    ///     Название события
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    ///     Описание события
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    ///     Начало события
    /// </summary>
    public required DateTime StartAt { get; init; }
    
    /// <summary>
    ///     Завершение события
    /// </summary>
    public required DateTime EndAt { get; init; }
    
    /// <summary>
    ///     Общее количество мест на событии;
    /// </summary>
    public required int? TotalSeats { get; init; }
}