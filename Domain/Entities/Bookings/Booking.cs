using Domain.Entities.Bookings.Parameters;
using Domain.Enums;

namespace Domain.Entities.Bookings;

public sealed class Booking
{
    private Booking () {}

    private Booking(CreateBookingParameters parameters) : this()
    {
        Id = parameters.Id;
        EventId = parameters.EventId;
        StartAt = DateTime.Now;
        Status = BookingStatus.Pending;
    }
    
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    ///     Внешний ключ событий
    /// </summary>
    public Guid EventId { get; set; }
    
    /// <summary>
    ///     Статус брони
    /// </summary>
    public BookingStatus Status { get; set; }
    
    /// <summary>
    ///     Начало события
    /// </summary>
    public DateTime StartAt { get; private set; }
    
    /// <summary>
    ///     Завершение события
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    ///     Добавить 
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Booking Create(CreateBookingParameters parameters) => new (parameters);

    /// <summary>
    ///     Подтвердить бронь
    /// </summary>
    public void ConfirmBooking()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.Now;
    }
    
    /// <summary>
    ///     Отклонить бронь
    /// </summary>
    public void RejectBooking()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.Now;
    }
}