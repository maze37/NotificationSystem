namespace NotificationSystem.Application.Abstractions;

/// <summary>
/// Абстракция публикации сообщений в транспорт.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Публикует событие о создании уведомления.
    /// </summary>
    Task PublishCreatedAsync(Guid notificationId, string correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Публикует событие повторной попытки отправки.
    /// </summary>
    Task PublishRetryAsync(Guid notificationId, string correlationId, int currentAttempt, CancellationToken cancellationToken);

    /// <summary>
    /// Публикует сообщение в очередь dead-letter.
    /// </summary>
    Task PublishDeadLetterAsync(Guid notificationId, string correlationId, string reason, CancellationToken cancellationToken);
}
