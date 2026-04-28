namespace NotificationSystem.Domain.Enums;

public enum NotificationStatus
{
    Created = 1,
    Queued = 2,
    Processing = 3,
    Delivered = 4,
    Failed = 5,
    DeadLettered = 6
}
