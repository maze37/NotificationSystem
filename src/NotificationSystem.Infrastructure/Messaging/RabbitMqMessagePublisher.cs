using System.Text;
using System.Text.Json;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Events;
using RabbitMQ.Client;

namespace NotificationSystem.Infrastructure.Messaging;

public sealed class RabbitMqMessagePublisher(IRabbitMqConnectionProvider connectionProvider) : IMessagePublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishCreatedAsync(Guid notificationId, string correlationId, CancellationToken cancellationToken) =>
        PublishAsync(
            MessagingTopology.ExchangeName,
            MessagingTopology.CreatedRoutingKey,
            new NotificationMessage(notificationId, correlationId),
            correlationId,
            cancellationToken);

    public Task PublishRetryAsync(Guid notificationId, string correlationId, int currentAttempt, CancellationToken cancellationToken)
    {
        var retryIndex = Math.Clamp(currentAttempt, 1, MessagingTopology.RetryQueues.Count);
        var routingKey = MessagingTopology.RetryQueues[retryIndex - 1].RoutingKey;

        return PublishAsync(
            MessagingTopology.ExchangeName,
            routingKey,
            new NotificationMessage(notificationId, correlationId),
            correlationId,
            cancellationToken);
    }

    public Task PublishDeadLetterAsync(Guid notificationId, string correlationId, string reason, CancellationToken cancellationToken) =>
        PublishAsync(
            MessagingTopology.DeadLetterExchangeName,
            MessagingTopology.DeadLetterRoutingKey,
            new DeadLetterNotificationMessage(notificationId, correlationId, reason),
            correlationId,
            cancellationToken);

    private Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        string correlationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var channel = connectionProvider.GetConnection().CreateModel();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, SerializerOptions));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.CorrelationId = correlationId;
        properties.ContentType = "application/json";

        channel.BasicPublish(exchange, routingKey, properties, body);
        return Task.CompletedTask;
    }
}
