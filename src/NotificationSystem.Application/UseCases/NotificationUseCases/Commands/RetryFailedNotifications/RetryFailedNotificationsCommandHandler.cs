using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

public sealed class RetryFailedNotificationsCommandHandler(
    INotificationRepository notificationRepository,
    IMessagePublisher messagePublisher,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IValidator<RetryFailedNotificationsCommand> validator)
{
    public async Task<Result<RetryFailedNotificationsResponse, Error>> HandleAsync(
        RetryFailedNotificationsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<RetryFailedNotificationsResponse>();
        }

        var threshold = dateTimeProvider.UtcNow.AddMinutes(-command.OlderThanMinutes);
        var batch = await notificationRepository.GetRecoveryBatchAsync(command.BatchSize, threshold, cancellationToken);

        var requeued = new List<Guid>(batch.Count);
        foreach (var notification in batch)
        {
            if (notification.Status == NotificationStatus.Created)
            {
                await messagePublisher.PublishCreatedAsync(notification.Id, notification.CorrelationId, cancellationToken);
                notification.MarkQueued(dateTimeProvider.UtcNow);
            }
            else if (notification.Status == NotificationStatus.Failed && !notification.IsRetryExhausted())
            {
                await messagePublisher.PublishRetryAsync(
                    notification.Id,
                    notification.CorrelationId,
                    notification.Attempts,
                    cancellationToken);
            }
            else
            {
                continue;
            }

            requeued.Add(notification.Id);
        }

        if (requeued.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success<RetryFailedNotificationsResponse, Error>(
            new RetryFailedNotificationsResponse(requeued.Count, requeued));
    }
}
