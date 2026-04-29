using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Application.DTOs;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Domain.Enums;
using NotificationSystem.Domain.Exceptions;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;

public sealed class ProcessNotificationCommandHandler : ICommandHandler<ProcessNotificationCommand, ProcessNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDeliveryAttemptRepository _deliveryAttemptRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITemplateServiceClient _templateServiceClient;
    private readonly IDeliveryServiceClient _deliveryServiceClient;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<ProcessNotificationCommand> _validator;

    public ProcessNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IDeliveryAttemptRepository deliveryAttemptRepository,
        IUnitOfWork unitOfWork,
        ITemplateServiceClient templateServiceClient,
        IDeliveryServiceClient deliveryServiceClient,
        IMessagePublisher messagePublisher,
        IDateTimeProvider dateTimeProvider,
        IValidator<ProcessNotificationCommand> validator)
    {
        _notificationRepository = notificationRepository;
        _deliveryAttemptRepository = deliveryAttemptRepository;
        _unitOfWork = unitOfWork;
        _templateServiceClient = templateServiceClient;
        _deliveryServiceClient = deliveryServiceClient;
        _messagePublisher = messagePublisher;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    public async Task<Result<ProcessNotificationResponse, Error>> HandleAsync(
        ProcessNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<ProcessNotificationResponse>();
        }

        var notification = await _notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken);
        if (notification is null)
        {
            return Result.Failure<ProcessNotificationResponse, Error>(
                Error.NotFound("notification.not_found", "Уведомление не найдено."));
        }

        if (notification.Status is NotificationStatus.Delivered or NotificationStatus.DeadLettered)
        {
            return Result.Success<ProcessNotificationResponse, Error>(new ProcessNotificationResponse(
                notification.ToResponse(),
                null,
                notification.Status == NotificationStatus.Delivered,
                false,
                notification.Status == NotificationStatus.DeadLettered,
                notification.ErrorMessage));
        }

        var now = _dateTimeProvider.UtcNow;
        int attemptNumber;
        DeliveryAttempt deliveryAttempt;
        try
        {
            attemptNumber = notification.StartProcessing(now);
            var deliveryAttemptResult = DeliveryAttempt.Start(Guid.NewGuid(), notification.Id, attemptNumber, now);
            if (deliveryAttemptResult.IsFailure)
            {
                return Result.Failure<ProcessNotificationResponse, Error>(deliveryAttemptResult.Error);
            }

            deliveryAttempt = deliveryAttemptResult.Value;
        }
        catch (DomainRuleException ex)
        {
            return Result.Failure<ProcessNotificationResponse, Error>(
                Error.Validation("notification.domain_rule", ex.Message));
        }

        await _deliveryAttemptRepository.AddAsync(deliveryAttempt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var rendered = await _templateServiceClient.RenderAsync(
                notification.Channel,
                notification.TemplateCode,
                notification.PayloadJson,
                cancellationToken);

            var dispatchResult = await _deliveryServiceClient.SendAsync(
                new DeliveryDispatchRequest(
                    notification.Channel,
                    notification.Recipient,
                    rendered.Subject,
                    rendered.Body,
                    notification.CorrelationId),
                cancellationToken);

            if (dispatchResult.IsSuccess)
            {
                deliveryAttempt.MarkDelivered();
                notification.MarkDelivered(_dateTimeProvider.UtcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success<ProcessNotificationResponse, Error>(new ProcessNotificationResponse(
                    notification.ToResponse(),
                    deliveryAttempt.ToResponse(),
                    true,
                    false,
                    false,
                    null));
            }

            return await HandleFailureAsync(
                notification,
                deliveryAttempt,
                dispatchResult.ErrorMessage ?? "Сервис доставки вернул неизвестную ошибку.",
                dispatchResult.IsTransientFailure,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return await HandleFailureAsync(
                notification,
                deliveryAttempt,
                ex.Message,
                true,
                cancellationToken);
        }
    }

    private async Task<Result<ProcessNotificationResponse, Error>> HandleFailureAsync(
        NotificationJob notification,
        DeliveryAttempt deliveryAttempt,
        string errorMessage,
        bool isTransientFailure,
        CancellationToken cancellationToken)
    {
        if (isTransientFailure && !notification.IsRetryExhausted())
        {
            deliveryAttempt.MarkFailed(errorMessage);
            notification.MarkFailed(errorMessage, _dateTimeProvider.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _messagePublisher.PublishRetryAsync(
                notification.Id,
                notification.CorrelationId,
                notification.Attempts,
                cancellationToken);

            return Result.Success<ProcessNotificationResponse, Error>(new ProcessNotificationResponse(
                notification.ToResponse(),
                deliveryAttempt.ToResponse(),
                false,
                true,
                false,
                errorMessage));
        }

        deliveryAttempt.MarkDeadLettered(errorMessage);
        notification.MarkDeadLettered(errorMessage, _dateTimeProvider.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _messagePublisher.PublishDeadLetterAsync(
            notification.Id,
            notification.CorrelationId,
            errorMessage,
            cancellationToken);

        return Result.Success<ProcessNotificationResponse, Error>(new ProcessNotificationResponse(
            notification.ToResponse(),
            deliveryAttempt.ToResponse(),
            false,
            false,
            true,
            errorMessage));
    }
}