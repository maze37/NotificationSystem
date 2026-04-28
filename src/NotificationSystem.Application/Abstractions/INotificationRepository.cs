using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.Abstractions;

public interface INotificationRepository
{
    Task AddAsync(NotificationJob notification, CancellationToken cancellationToken);

    Task<NotificationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<NotificationJob?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificationJob>> SearchAsync(
        NotificationStatus? status,
        NotificationChannel? channel,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotificationJob>> GetRecoveryBatchAsync(
        int limit,
        DateTimeOffset olderThanUtc,
        CancellationToken cancellationToken);
}
