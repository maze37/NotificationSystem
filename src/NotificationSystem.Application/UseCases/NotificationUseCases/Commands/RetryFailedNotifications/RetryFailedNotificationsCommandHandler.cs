using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

public sealed class RetryFailedNotificationsCommandHandler : ICommandHandler<RetryFailedNotificationsCommand, RetryFailedNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<RetryFailedNotificationsCommand> _validator;

    public RetryFailedNotificationsCommandHandler(
        INotificationRepository notificationRepository,
        IMessagePublisher messagePublisher,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IValidator<RetryFailedNotificationsCommand> validator)
    {
        _notificationRepository = notificationRepository;
        _messagePublisher = messagePublisher;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    public async Task<Result<RetryFailedNotificationsResponse, Error>> HandleAsync(
        RetryFailedNotificationsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<RetryFailedNotificationsResponse>();
        }

        var threshold = _dateTimeProvider.UtcNow.AddMinutes(-command.OlderThanMinutes);
        var batch = await _notificationRepository.GetRecoveryBatchAsync(command.BatchSize, threshold, cancellationToken);

        var requeued = new List<Guid>(batch.Count);
        foreach (var notification in batch)
        {
            if (notification.Status == NotificationStatus.Created)
            {
                await _messagePublisher.PublishCreatedAsync(notification.Id, notification.CorrelationId, cancellationToken);
                notification.MarkQueued(_dateTimeProvider.UtcNow);
            }
            else if (notification.Status == NotificationStatus.Failed && !notification.IsRetryExhausted())
            {
                await _messagePublisher.PublishRetryAsync(
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success<RetryFailedNotificationsResponse, Error>(
            new RetryFailedNotificationsResponse(requeued.Count, requeued));
    }
}