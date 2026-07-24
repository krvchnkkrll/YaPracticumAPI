namespace Events.Domain.Entities.Events.Parameters;

public readonly struct ValidateForCreateParameter
{
    /// <summary>
    ///     Название события
    /// </summary>
    public required string Title { get; init; }
    
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
}