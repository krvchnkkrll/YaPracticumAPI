using System.Text.Json;
using Bookings.Application.Interfaces.Events;
using Bookings.Infrastructure.Models;
using Confluent.Kafka;
using Kafka.Contracts.Constance;
using Kafka.Contracts.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookings.Infrastructure.Events;

internal sealed class KafkaBookingConfirmedPublisher(
    IOptions<KafkaOptions> options,
    ILogger<KafkaBookingConfirmedPublisher> logger)
    : IBookingConfirmedPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        new ProducerConfig { BootstrapServers = options.Value.BootstrapServers }).Build();

    public async Task PublishAsync(BookingConfirmedEvent @event, CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = @event.EventId.ToString(),
            Value = JsonSerializer.Serialize(@event),
        };

        try
        {
            await _producer.ProduceAsync(Topics.BookingConfirmTopic, message, cancellationToken);
        }
        catch (ProduceException<string, string> e)
        {
            logger.LogError(
                e,
                "Не удалось опубликовать {EventType} по брони {BookingId} в топик {Topic}",
                nameof(BookingConfirmedEvent),
                @event.BookingId,
                Topics.BookingConfirmTopic);

            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
