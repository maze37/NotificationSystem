using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;

public sealed record SearchNotificationsQuery(
    NotificationStatus? Status,
    NotificationChannel? Channel,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc) : IQuery<IReadOnlyCollection<NotificationResponse>>;
