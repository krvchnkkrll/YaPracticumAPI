namespace IntegrationTests.Fixtures;

internal static class DateTimeExtensions
{
    public static DateTime TruncateToMicroseconds(this DateTime value)
        => new(value.Ticks - value.Ticks % TimeSpan.TicksPerMicrosecond, value.Kind);
}
