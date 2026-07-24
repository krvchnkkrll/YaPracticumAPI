namespace Kafka.Contracts.Constance;

public static class Topics
{
    /// <summary>
    ///     Название топика для подтверждения брони
    /// </summary>
    public const string BookingConfirmTopic = "booking-confirmed";

    /// <summary>
    ///     Название топика для отмены брони
    /// </summary>
    public const string BookingCancelTopic = "booking-cancelled";
}