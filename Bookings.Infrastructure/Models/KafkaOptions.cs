namespace Bookings.Infrastructure.Models;

internal sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
}
