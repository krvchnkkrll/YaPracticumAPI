using System.Text.Json;
using Confluent.Kafka;
using Events.Application.Interfaces.Repositories;
using Events.Domain.Exceptions;
using Events.Infrastructure.Models;
using Kafka.Contracts.Constance;
using Kafka.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Events.Infrastructure.Events;

internal sealed class KafkaBookingConfirmedConsumerWorker(
    IOptions<KafkaOptions> options,
    ILogger<KafkaBookingConfirmedConsumerWorker> logger,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Booking Confirmed Consumer Worker starting...");

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka Booking Confirmed Consumer Worker is stopping...");

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

        consumer.Subscribe(Topics.BookingConfirmTopic);

        logger.LogInformation("Kafka Booking Confirmed Consumer starting...");

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
                    logger.LogError(e, "Ошибка чтения сообщения из топика {Topic}.", Topics.BookingConfirmTopic);
                }
                catch (KeyNotFoundException e)
                {
                    logger.LogWarning(e, "Событие для полученного сообщения не найдено.");
                }
                catch (EventAlreadyStartedException e)
                {
                    logger.LogWarning(e, "Событие уже началось.");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Не удалось обработать сообщение из топика {Topic}.", Topics.BookingConfirmTopic);
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
            logger.LogInformation("Kafka Booking Confirmed Consumer stopping...");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
    {
        var bookingConfirmedEvent = JsonSerializer.Deserialize<BookingConfirmedEvent>(consumeResult.Message.Value)
            ?? throw new InvalidOperationException("BookingConfirmedEvent не удалось десериализовать.");

        using var scope = scopeFactory.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var eventEntity = await eventRepository.GetByIdAsync(bookingConfirmedEvent.EventId, stoppingToken);

        if (!eventEntity.TryReserveSeats(bookingConfirmedEvent.SeatsCount))
        {
            logger.LogWarning(
                "Недостаточно свободных мест у события {EventId} для брони {BookingId}.",
                bookingConfirmedEvent.EventId,
                bookingConfirmedEvent.BookingId);

            return;
        }

        await eventRepository.SaveChangesAsync(stoppingToken);
    }
}
