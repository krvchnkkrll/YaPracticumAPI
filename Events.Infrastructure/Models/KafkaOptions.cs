namespace Events.Infrastructure.Models;

internal sealed class KafkaOptions
{
    public required string BootstrapServers { get; init; }
    public required string ConsumerGroup { get; init; }
}
