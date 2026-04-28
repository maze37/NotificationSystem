using Microsoft.Extensions.Options;
using NotificationSystem.Contracts.Events;
using NotificationSystem.Infrastructure.Options;
using RabbitMQ.Client;

namespace NotificationSystem.Infrastructure.Messaging;

public sealed class RabbitMqTopologyInitializer(
    IRabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options)
{
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (options.Value.RetryDelaysSeconds.Count == 0)
        {
            throw new InvalidOperationException("RabbitMq:RetryDelaysSeconds должен содержать минимум одно значение.");
        }

        using var channel = connectionProvider.GetConnection().CreateModel();

        channel.ExchangeDeclare(MessagingTopology.ExchangeName, ExchangeType.Direct, durable: true);
        channel.ExchangeDeclare(MessagingTopology.DeadLetterExchangeName, ExchangeType.Direct, durable: true);

        channel.QueueDeclare(MessagingTopology.CreatedQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(MessagingTopology.CreatedQueue, MessagingTopology.ExchangeName, MessagingTopology.CreatedRoutingKey);

        channel.QueueDeclare(MessagingTopology.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(MessagingTopology.DeadLetterQueue, MessagingTopology.DeadLetterExchangeName, MessagingTopology.DeadLetterRoutingKey);

        for (var index = 0; index < MessagingTopology.RetryQueues.Count; index++)
        {
            var retryQueue = MessagingTopology.RetryQueues[index];
            var ttlSeconds = index < options.Value.RetryDelaysSeconds.Count
                ? options.Value.RetryDelaysSeconds[index]
                : options.Value.RetryDelaysSeconds.Last();

            var arguments = new Dictionary<string, object>
            {
                ["x-message-ttl"] = ttlSeconds * 1000,
                ["x-dead-letter-exchange"] = MessagingTopology.ExchangeName,
                ["x-dead-letter-routing-key"] = MessagingTopology.CreatedRoutingKey
            };

            channel.QueueDeclare(retryQueue.QueueName, durable: true, exclusive: false, autoDelete: false, arguments);
            channel.QueueBind(retryQueue.QueueName, MessagingTopology.ExchangeName, retryQueue.RoutingKey);
        }

        return Task.CompletedTask;
    }
}
