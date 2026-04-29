using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Enums;
using NotificationSystem.Domain.Exceptions;
using Entity = NotificationSystem.Contracts.Base.Entity;

namespace NotificationSystem.Domain.Entities;

/// <summary>
/// Попытка доставки конкретного уведомления.
/// Время и идентификатор поставляются из Application-слоя.
/// </summary>
public sealed class DeliveryAttempt : Entity
{
    private DeliveryAttempt()
    {
    }

    private DeliveryAttempt(Guid id, Guid notificationId, int attemptNumber, DateTimeOffset createdWhen) : base(id)
    {
        NotificationId = notificationId;
        AttemptNumber = attemptNumber;
        Status = NotificationStatus.Processing;
        CreatedWhen = createdWhen;
    }

    public Guid NotificationId { get; private set; }

    public NotificationJob? Notification { get; private set; }

    public int AttemptNumber { get; private set; }

    public NotificationStatus Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset CreatedWhen { get; private set; }

    public static Result<DeliveryAttempt, Error> Start(Guid id, Guid notificationId, int attemptNumber, DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<DeliveryAttempt, Error>(
                Error.Validation("delivery_attempt.id.invalid", "Идентификатор попытки доставки не может быть пустым.", nameof(id)));
        }

        if (notificationId == Guid.Empty)
        {
            return Result.Failure<DeliveryAttempt, Error>(
                Error.Validation("delivery_attempt.notification_id.invalid", "Идентификатор уведомления не может быть пустым.", nameof(notificationId)));
        }

        if (attemptNumber <= 0)
        {
            return Result.Failure<DeliveryAttempt, Error>(
                Error.Validation("delivery_attempt.attempt_number.invalid", "Номер попытки должен быть больше нуля.", nameof(attemptNumber)));
        }

        return Result.Success<DeliveryAttempt, Error>(
            new DeliveryAttempt(id, notificationId, attemptNumber, createdWhen));
    }

    public void MarkDelivered()
    {
        EnsureProcessingTransition(NotificationStatus.Delivered);
        Status = NotificationStatus.Delivered;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        EnsureProcessingTransition(NotificationStatus.Failed);
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void MarkDeadLettered(string errorMessage)
    {
        EnsureProcessingTransition(NotificationStatus.DeadLettered);
        Status = NotificationStatus.DeadLettered;
        ErrorMessage = errorMessage;
    }

    private void EnsureProcessingTransition(NotificationStatus nextStatus)
    {
        if (Status != NotificationStatus.Processing)
        {
            throw new DomainRuleException($"Нельзя перевести попытку доставки {Id} из {Status} в {nextStatus}.");
        }
    }
}