using Domain.Entities.Bookings;
using Domain.Entities.Bookings.Parameters;
using Domain.Entities.Events.Parameters;
using Domain.Exceptions;

namespace Domain.Entities.Events;

public sealed class Event
{
    private Event() {}

    private Event(CreateEventParameter parameter) : this()
    {
        ValidateForCreate(new ValidateForCreateParameter
        {
            Title = parameter.Title,
            StartAt = parameter.StartAt,
            EndAt = parameter.EndAt,
            TotalSeats = parameter.TotalSeats,
        });
        
        Title = parameter.Title;
        Description = parameter.Description;
        StartAt = parameter.StartAt;
        EndAt = parameter.EndAt;
        TotalSeats = parameter.TotalSeats;
        AvailableSeats = parameter.TotalSeats;
    }

    /// <summary>
    ///     Идентификатор
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
    
    /// <summary>
    ///     Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; private set; }
    
    /// <summary>
    ///     Доступное количество мест на событии
    /// </summary>
    public int AvailableSeats { get; private set; }
    
    private readonly List<Booking> _bookings = [];

    /// <summary>
    ///     Брони
    /// </summary>
    public IReadOnlyCollection<Booking> Bookings => _bookings;
    
    /// <summary>
    ///     Создать событие
    /// </summary>
    public static Event Create(CreateEventParameter parameter) => new(parameter);

    /// <summary>
    ///     Обновить событие
    /// </summary>
    public void Update(UpdateEventParameter parameter)
    {
        ValidateForUpdate(new ValidateForUpdateParameter
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
    
    /// <summary>
    ///     Добавить 
    /// </summary>
    public Booking CreateBooking(Guid userId)
    { 
        if (StartAt <= DateTime.UtcNow)
            throw new EventAlreadyStartedException();
        
        var booking = Booking.Create(new CreateBookingParameters
        {
            EventId = Id,
            UserId = userId,
        });
        _bookings.Add(booking);
        return booking;
    }
    
    private static void ValidateForCreate(ValidateForCreateParameter forCreateParameter)
    {
        if (string.IsNullOrWhiteSpace(forCreateParameter.Title))
            throw new ArgumentException("Название события обязательно и не может быть пустым.");

        if (forCreateParameter.StartAt >= forCreateParameter.EndAt)
            throw new ArgumentException("Дата начала события должна быть раньше даты завершения.");

        if (forCreateParameter.TotalSeats < 0)
            throw new ArgumentException("Общее количество мест на событии не может быть меньше нуля.");
        
        if (forCreateParameter.TotalSeats == 0)
            throw new ArgumentException("Общее количество мест на событии не может быть равно нулю.");
    }
    
    private static void ValidateForUpdate(ValidateForUpdateParameter forCreateParameter)
    {
        if (string.IsNullOrWhiteSpace(forCreateParameter.Title))
            throw new ArgumentException("Название события обязательно и не может быть пустым.");

        if (forCreateParameter.StartAt >= forCreateParameter.EndAt)
            throw new ArgumentException("Дата начала события должна быть раньше даты завершения.");
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0) 
            throw new ArgumentOutOfRangeException(nameof(count), "Количество мест для бронирования должно быть больше нуля.");

        if (AvailableSeats < count)
            return false;

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Количество освобождаемых мест должно быть больше нуля.");

        if (AvailableSeats + count > TotalSeats) 
            throw new InvalidOperationException("Количество доступных мест не может быть больше общего количества мест.");

        AvailableSeats += count;
    }
}