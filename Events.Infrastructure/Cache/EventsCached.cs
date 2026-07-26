using System.Text.Json;
using Events.Application.Interfaces.Cache;
using Events.Domain.Entities.Events;
using Events.Infrastructure.Constants;
using Events.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Events.Infrastructure.Cache;

public sealed class EventsCached(
    IDatabase database, 
    IOptions<RedisOptions> options, 
    ILogger<EventsCached> logger) 
    : IEventsCached
{
    private readonly RedisOptions _options = options.Value;

    public async Task<IEnumerable<Event>> GetCachedTopEventsAsync()
    {
        try
        {
            var cached = await database.StringGetAsync(RedisKeyNames.TopEvents);

            if (!cached.HasValue)
                return [];

            return JsonSerializer.Deserialize<IEnumerable<Event>>(cached!) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось прочитать топ событий из кеша");
            return [];
        }
    }

    public async Task<Event?> GetCachedEventByIdAsync(Guid eventId)
    {
        var cacheKey = RedisKeyNames.EventById(eventId);

        try
        {
            var cached = await database.StringGetAsync(cacheKey);

            return cached.HasValue
                ? JsonSerializer.Deserialize<Event>(cached!)
                : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось прочитать событие {CacheKey} из кеша", cacheKey);
            return null;
        }
    }

    public async Task CreateCacheEventAsync(Event @event)
    {
        var cacheKey = RedisKeyNames.EventById(@event.Id);

        try
        {
            var json = JsonSerializer.Serialize(@event);

            var result = await database.StringSetAsync(cacheKey, json, TimeSpan.FromSeconds(_options.EventTtlSeconds));

            if (!result)
                logger.LogError("Не удалось закешировать событие: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось закешировать событие: {CacheKey}", cacheKey);
        }
    }

    public async Task CreateTopCacheEventsAsync(IReadOnlyList<Event> events)
    {
        try
        {
            var json = JsonSerializer.Serialize(events);

            var result = await database.StringSetAsync(RedisKeyNames.TopEvents, json, TimeSpan.FromSeconds(_options.TopEventsTtlSeconds));

            if (!result)
                logger.LogError("Не удалось закешировать топ событий: {CacheKey}", RedisKeyNames.TopEvents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось закешировать топ событий: {CacheKey}", RedisKeyNames.TopEvents);
        }
    }

    public async Task RemoveCachedEventAsync(Guid eventId)
    {
        var cacheKey = RedisKeyNames.EventById(eventId);

        try
        {
            await database.KeyDeleteAsync(cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось инвалидировать кеш: {CacheKey}", cacheKey);
        }
    }
}
