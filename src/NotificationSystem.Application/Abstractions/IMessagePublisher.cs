namespace NotificationSystem.Application.Abstractions;

public interface IMessagePublisher
{
    Task PublishCreatedAsync(Guid notificationId, string correlationId, CancellationToken cancellationToken);

    Task PublishRetryAsync(Guid notificationId, string correlationId, int currentAttempt, CancellationToken cancellationToken);

    Task PublishDeadLetterAsync(Guid notificationId, string correlationId, string reason, CancellationToken cancellationToken);
}
