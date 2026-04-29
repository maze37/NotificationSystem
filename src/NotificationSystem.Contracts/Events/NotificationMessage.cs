namespace NotificationSystem.Contracts.Events;

/// <summary>
/// Сообщение очереди о том, что уведомление нужно обработать.
/// </summary>
public sealed record NotificationMessage(Guid NotificationId, string CorrelationId);
