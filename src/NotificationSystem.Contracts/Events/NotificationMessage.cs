namespace NotificationSystem.Contracts.Events;

public sealed record NotificationMessage(Guid NotificationId, string CorrelationId);
