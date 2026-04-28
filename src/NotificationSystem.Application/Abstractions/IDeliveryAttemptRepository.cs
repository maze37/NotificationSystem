using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Abstractions;

public interface IDeliveryAttemptRepository
{
    Task AddAsync(DeliveryAttempt attempt, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DeliveryAttempt>> GetByNotificationIdAsync(Guid notificationId, CancellationToken cancellationToken);
}
