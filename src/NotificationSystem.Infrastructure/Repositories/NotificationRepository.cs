using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Enums;
using NotificationSystem.Infrastructure.Persistence;

namespace NotificationSystem.Infrastructure.Repositories;

public sealed class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public Task AddAsync(NotificationJob notification, CancellationToken cancellationToken) =>
        dbContext.NotificationJobs.AddAsync(notification, cancellationToken).AsTask();

    public Task<NotificationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.NotificationJobs
            .Include(x => x.DeliveryAttempts)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<NotificationJob?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken) =>
        dbContext.NotificationJobs
            .SingleOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);

    public async Task<IReadOnlyCollection<NotificationJob>> SearchAsync(
        NotificationStatus? status,
        NotificationChannel? channel,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var query = dbContext.NotificationJobs.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (channel.HasValue)
        {
            query = query.Where(x => x.Channel == channel.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedWhen >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedWhen <= toUtc.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedWhen)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationJob>> GetRecoveryBatchAsync(
        int limit,
        DateTimeOffset olderThanUtc,
        CancellationToken cancellationToken)
    {
        return await dbContext.NotificationJobs
            .Where(x =>
                (x.Status == NotificationStatus.Created || x.Status == NotificationStatus.Failed) &&
                x.UpdatedWhen <= olderThanUtc)
            .OrderBy(x => x.UpdatedWhen)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }
}
