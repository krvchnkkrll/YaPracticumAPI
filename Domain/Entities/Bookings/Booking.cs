using Domain.Entities.Bookings.Parameters;
using Domain.Entities.Events;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Bookings;

public sealed class Booking
{
    private Booking () {}

    private Booking(CreateBookingParameters parameters) : this()
    {
 
        EventId = parameters.EventId;
        CreatedAt = DateTime.UtcNow;
        Status = BookingStatus.Pending;
        UserId = parameters.UserId;
    }
    
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    
    /// <summary>
    ///     Статус брони
    /// </summary>
    public BookingStatus Status { get; private set; }
    
    /// <summary>
    ///     Время создание брони
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    ///     Время завершение обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }
    
    /// <summary>
    ///     Внешний ключ событий
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    ///     Навигационное свойство событий
    /// </summary>
    public Event Event { get; private set; } = null!;
    
    /// <summary>
    ///     Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    ///     Навигационное свойство пользователя
    /// </summary>
    public User User { get; private set; } = null!;
    

    /// <summary>
    ///     Добавить 
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    internal static Booking Create(CreateBookingParameters parameters) => new (parameters);

    /// <summary>
    ///     Подтвердить бронь
    /// </summary>
    public void ConfirmBooking()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    ///     Отклонить бронь
    /// </summary>
    public void RejectBooking()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    ///     Отклонить бронь
    /// </summary>
    public void CancelledBooking()
    {
        if (Status == BookingStatus.Cancelled)
            return;
        
        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }
}