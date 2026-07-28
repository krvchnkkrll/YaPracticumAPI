namespace Events.Infrastructure.Constants;

public static class RedisKeyNames
{
    public const string TopEvents = "events:top10";

    public static string EventById(Guid eventId) => $"event:{eventId}";
}
