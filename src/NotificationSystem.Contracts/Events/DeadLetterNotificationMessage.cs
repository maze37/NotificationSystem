namespace NotificationSystem.Contracts.Events;

public sealed record DeadLetterNotificationMessage(Guid NotificationId, string CorrelationId, string Reason);
