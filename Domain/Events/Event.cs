namespace Domain.Events;

public sealed class Event
{
    /// <summary>
    ///     Идентификатор события
    /// </summary>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Название события
    /// </summary>
    public string Title { get; private set; } = null!;
    
    /// <summary>
    ///     Описание события
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    ///     Начало события
    /// </summary>
    public DateTime StartAt { get; private set; }
    
    /// <summary>
    ///     Завершение события
    /// </summary>
    public DateTime EndAt { get; private set; }
}