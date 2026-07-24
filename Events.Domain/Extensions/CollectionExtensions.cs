namespace Events.Domain.Extensions;

public static class CollectionExtensions
{
    public static string CollectionToString(this IList<string> collection)
    {
        return string.Join(", ", collection);
    }
}