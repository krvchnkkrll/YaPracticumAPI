namespace Application.Models;

public sealed class GetEventResponse
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    ///     Название события
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    ///     Описание события
    /// </summary>
    public required string? Description { get; init; }
    
    /// <summary>
    ///     Начало события
    /// </summary>
    public required DateTime StartAt { get; init; }
    
    /// <summary>
    ///     Завершение события
    /// </summary>
    public required DateTime EndAt { get; init; }

    /// <summary>
    ///     Общее количество мест
    /// </summary>
    public required int TotalSeats { get; init; }
    
    /// <summary>
    ///     Количество доступных мест
    /// </summary>
    public required int AvailableSeats { get; init; }
}