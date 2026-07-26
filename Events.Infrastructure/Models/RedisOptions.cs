namespace Events.Infrastructure.Models;

public sealed class RedisOptions
{
    public int EventTtlSeconds { get; init; }
    public int TopEventsTtlSeconds { get; init; }
}
