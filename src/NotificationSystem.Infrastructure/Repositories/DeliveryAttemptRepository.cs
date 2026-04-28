using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Infrastructure.Persistence;

namespace NotificationSystem.Infrastructure.Repositories;

public sealed class DeliveryAttemptRepository(NotificationDbContext dbContext) : IDeliveryAttemptRepository
{
    public Task AddAsync(DeliveryAttempt attempt, CancellationToken cancellationToken) =>
        dbContext.DeliveryAttempts.AddAsync(attempt, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<DeliveryAttempt>> GetByNotificationIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.DeliveryAttempts
            .AsNoTracking()
            .Where(x => x.NotificationId == notificationId)
            .OrderBy(x => x.AttemptNumber)
            .ToArrayAsync(cancellationToken);
    }
}
