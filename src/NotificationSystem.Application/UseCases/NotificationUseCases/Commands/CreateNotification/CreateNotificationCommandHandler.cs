using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;

public sealed class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand, CreateNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<CreateNotificationCommand> _validator;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher,
        IDateTimeProvider dateTimeProvider,
        IValidator<CreateNotificationCommand> validator)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    public async Task<Result<CreateNotificationResponse, Error>> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<CreateNotificationResponse>();
        }

        var existing = await _notificationRepository.GetByCorrelationIdAsync(command.CorrelationId, cancellationToken);
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
            _dateTimeProvider.UtcNow);

        if (notificationResult.IsFailure)
        {
            return Result.Failure<CreateNotificationResponse, Error>(notificationResult.Error);
        }
        var notification = notificationResult.Value;

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _messagePublisher.PublishCreatedAsync(notification.Id, notification.CorrelationId, cancellationToken);
        notification.MarkQueued(_dateTimeProvider.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<CreateNotificationResponse, Error>(
            new CreateNotificationResponse(notification.ToResponse(), false));
    }
}