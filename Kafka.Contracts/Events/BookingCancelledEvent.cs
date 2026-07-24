namespace Kafka.Contracts.Events;

public sealed class BookingCancelledEvent
{
    /// <summary>
    ///     Идентификатор брони
    /// </summary>
    public required Guid BookingId { get; init; }

    /// <summary>
    ///     Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    ///     Идентификатор события
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    ///     Сколько мест освобождается
    /// </summary>
    public required int SeatsCount { get; init; }

    /// <summary>
    ///     Отменено в
    /// </summary>
    public required DateTime CancelledAt { get; init; }
}
