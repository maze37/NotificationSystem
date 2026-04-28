using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;

public sealed record SearchNotificationsQuery(
    NotificationStatus? Status,
    NotificationChannel? Channel,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc);
