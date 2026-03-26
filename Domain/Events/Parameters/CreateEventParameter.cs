namespace Domain.Events.Parameters;

public readonly struct CreateEventParameter
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
}