using System.Text.Json;
using Confluent.Kafka;
using Events.Application.Interfaces.Cache;
using Events.Application.Interfaces.Repositories;
using Events.Infrastructure.Models;
using Kafka.Contracts.Constance;
using Kafka.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Events.Infrastructure.Events;

internal sealed class KafkaBookingCancelledConsumerWorker(
    IOptions<KafkaOptions> options,
    ILogger<KafkaBookingCancelledConsumerWorker> logger,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Booking Cancelled Consumer Worker starting...");

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Booking Cancelled Consumer Worker is stopping...");

        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }

    private async Task Consume(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.ConsumerGroup,
            EnableAutoOffsetStore = false,
        }).Build();

        consumer.Subscribe(Topics.BookingCancelTopic);

        logger.LogInformation("Kafka Booking Cancelled Consumer starting...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult = null;

                try
                {
                    consumeResult = consumer.Consume(stoppingToken);

                    await ProcessMessageAsync(consumeResult, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Ошибка чтения сообщения из топика {Topic}.", Topics.BookingCancelTopic);
                }
                catch (KeyNotFoundException e)
                {
                    logger.LogWarning(e, "Событие для полученного сообщения не найдено.");
                }
                catch (InvalidOperationException e)
                {
                    logger.LogWarning(e, "Не удалось освободить места — вероятно, сообщение уже было обработано ранее.");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Не удалось обработать сообщение из топика {Topic}.", Topics.BookingCancelTopic);
                }
                finally
                {
                    if (consumeResult is not null)
                        consumer.StoreOffset(consumeResult);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Kafka Booking Cancelled Consumer stopping...");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
    {
        var bookingCancelledEvent = JsonSerializer.Deserialize<BookingCancelledEvent>(consumeResult.Message.Value)
            ?? throw new FormatException("BookingCancelledEvent не удалось десериализовать.");

        using var scope = scopeFactory.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var eventCached = scope.ServiceProvider.GetRequiredService<IEventCached>();

        var eventEntity = await eventRepository.GetByIdAsync(bookingCancelledEvent.EventId, stoppingToken);

        eventEntity.ReleaseSeats(bookingCancelledEvent.SeatsCount);

        await eventRepository.SaveChangesAsync(stoppingToken);

        await eventCached.RemoveCachedEventAsync(bookingCancelledEvent.EventId);
    }
}