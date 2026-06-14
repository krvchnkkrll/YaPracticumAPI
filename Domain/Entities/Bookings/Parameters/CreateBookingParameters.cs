namespace Domain.Entities.Bookings.Parameters;

public struct CreateBookingParameters
{
    /// <summary>
    ///     Внешний ключ события
    /// </summary>
    public required Guid EventId { get; init; }
}