using Domain.Entities.Events.Parameters;

namespace Domain.Entities.Events;

public sealed class Event
{
    private Event() {}

    private Event(CreateEventParameter parameter) : this()
    {
        Validate(new ValidateParameter
        {
            Title = parameter.Title,
            StartAt = parameter.StartAt,
            EndAt = parameter.EndAt,
        });
        
        Id = parameter.Id;
        Title = parameter.Title;
        Description = parameter.Description;
        StartAt = parameter.StartAt;
        EndAt = parameter.EndAt;
    }

    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; private set; }

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

    /// <summary>
    ///     Создать событие
    /// </summary>
    public static Event Create(CreateEventParameter parameter) => new(parameter);

    /// <summary>
    ///     Обновить событие
    /// </summary>
    public void Update(UpdateEventParameter parameter)
    {
        Validate(new ValidateParameter
        {
            Title = parameter.Title,
            StartAt = parameter.StartAt,
            EndAt = parameter.EndAt,
        });
        
        Title = parameter.Title;
        Description = parameter.Description;
        StartAt = parameter.StartAt;
        EndAt = parameter.EndAt;
    }
    
    private static void Validate(ValidateParameter parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter.Title))
            throw new ArgumentException("Название события обязательно и не может быть пустым.");

        if (parameter.StartAt >= parameter.EndAt)
            throw new ArgumentException("Дата начала события должна быть раньше даты завершения.");
    }
}