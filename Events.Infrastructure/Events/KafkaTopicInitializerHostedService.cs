using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Events.Domain.Extensions;
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

        string[] topics = [Topics.BookingConfirmTopic, Topics.BookingCancelTopic];

        try
        {
            await adminClient.CreateTopicsAsync(topics.Select(topic => new TopicSpecification
            {
                Name = topic,
                NumPartitions = 3,
                ReplicationFactor = 1,
            }));

            logger.LogInformation("Топики {Topics} созданы", topics.CollectionToString());
        }
        catch (CreateTopicsException e) when (e.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            logger.LogInformation("Топики {Topics} уже существуют", topics.CollectionToString());
        }
        catch (CreateTopicsException e)
        {
            foreach (var result in e.Results.Where(r => r.Error.Code != ErrorCode.NoError && r.Error.Code != ErrorCode.TopicAlreadyExists))
                logger.LogError("Не удалось создать топик {Topic}: {Error}", result.Topic, result.Error.Reason);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось создать топики {Topics}", topics.CollectionToString());
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}