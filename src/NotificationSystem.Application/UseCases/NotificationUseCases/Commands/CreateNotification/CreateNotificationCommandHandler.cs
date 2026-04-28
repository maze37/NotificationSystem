using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    IMessagePublisher messagePublisher,
    IDateTimeProvider dateTimeProvider,
    IValidator<CreateNotificationCommand> validator)
{
    public async Task<Result<CreateNotificationResponse, Error>> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<CreateNotificationResponse>();
        }

        var existing = await notificationRepository.GetByCorrelationIdAsync(command.CorrelationId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success<CreateNotificationResponse, Error>(new CreateNotificationResponse(existing.ToResponse(), true));
        }

        var notificationResult = NotificationJob.Create(
            Guid.NewGuid(),
            command.Channel,
            command.Recipient,
            command.TemplateCode,
            command.PayloadJson,
            command.CorrelationId,
            dateTimeProvider.UtcNow);

        if (notificationResult.IsFailure)
        {
            return Result.Failure<CreateNotificationResponse, Error>(notificationResult.Error);
        }
        var notification = notificationResult.Value;

        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await messagePublisher.PublishCreatedAsync(notification.Id, notification.CorrelationId, cancellationToken);
        notification.MarkQueued(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<CreateNotificationResponse, Error>(
            new CreateNotificationResponse(notification.ToResponse(), false));
    }
}
