using Bookings.Domain.Enums;

namespace Bookings.Application.Models;

public sealed class CreateBookingResponse
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Внешний ключ события
    /// </summary>
    public required Guid EventId { get; init; }
    
    /// <summary>
    ///     Статус брони
    /// </summary>
    public required BookingStatus Status { get; init; }
}