namespace Bookings.Domain.Entities.Bookings.Parameters;

public struct CreateBookingParameters
{
    /// <summary>
    ///     Внешний ключ события
    /// </summary>
    public required Guid EventId { get; init; }
    
    /// <summary>
    ///     Внешний ключ пользователя
    /// </summary>
    public required Guid UserId { get; init; }
}