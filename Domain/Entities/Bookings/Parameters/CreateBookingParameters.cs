namespace Domain.Entities.Bookings.Parameters;

public struct CreateBookingParameters
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    ///     Внешний ключ события
    /// </summary>
    public required Guid EventId { get; init; }
}