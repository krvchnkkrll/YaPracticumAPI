using System.Text.Json;
using Bookings.Application.Interfaces.Events;
using Bookings.Infrastructure.Models;
using Confluent.Kafka;
using Kafka.Contracts.Constance;
using Kafka.Contracts.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookings.Infrastructure.Events;

internal sealed class KafkaBookingEventPublisher(
    IOptions<KafkaOptions> options,
    ILogger<KafkaBookingEventPublisher> logger)
    : IBookingEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        new ProducerConfig { BootstrapServers = options.Value.BootstrapServers }).Build();

    public Task PublishBookingConfirmedAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken) =>
        PublishAsync(Topics.BookingConfirmTopic, @event.EventId, @event.BookingId, @event, cancellationToken);

    public Task PublishBookingCancelledAsync(BookingCancelledEvent @event, CancellationToken cancellationToken) =>
        PublishAsync(Topics.BookingCancelTopic, @event.EventId, @event.BookingId, @event, cancellationToken);

    private async Task PublishAsync<TEvent>(string topic, Guid eventId, Guid bookingId, TEvent @event,
        CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = eventId.ToString(),
            Value = JsonSerializer.Serialize(@event),
        };

        try
        {
            await _producer.ProduceAsync(topic, message, cancellationToken);
        }
        catch (ProduceException<string, string> e)
        {
            logger.LogError(e, "Ошибка публикации {EventType} по брони {BookingId} в топик {Topic}",
                typeof(TEvent).Name, bookingId, topic);

            throw;
        }
    }

    public void Dispose()
    {
        var remaining = _producer.Flush(TimeSpan.FromSeconds(10));

        if (remaining > 0)
            logger.LogWarning("При остановке продюсера не отправлено {Count} сообщений", remaining);

        _producer.Dispose();
    }
}