using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Events.Infrastructure.Models;
using Kafka.Contracts.Constance;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Events.Infrastructure.Events;

internal sealed class KafkaTopicInitializerHostedService(
    IOptions<KafkaOptions> options,
    ILogger<KafkaTopicInitializerHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
        }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = Topics.BookingConfirmTopic,
                    NumPartitions = 3,
                    ReplicationFactor = 1,
                }
            ]);

            logger.LogInformation("Топик {Topic} создан", Topics.BookingConfirmTopic);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось создать топик {Topic}", Topics.BookingConfirmTopic);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}