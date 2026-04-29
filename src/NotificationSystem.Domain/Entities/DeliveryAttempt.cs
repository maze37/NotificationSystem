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

    /// <summary>
    /// Идентификатор уведомления, к которому относится попытка.
    /// </summary>
    public Guid NotificationId { get; private set; }

    /// <summary>
    /// Навигация к уведомлению.
    /// </summary>
    public NotificationJob? Notification { get; private set; }

    /// <summary>
    /// Порядковый номер попытки.
    /// </summary>
    public int AttemptNumber { get; private set; }

    /// <summary>
    /// Результат текущей попытки.
    /// </summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>
    /// Ошибка попытки, если доставка неуспешна.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Время создания попытки.
    /// </summary>
    public DateTimeOffset CreatedWhen { get; private set; }

    /// <summary>
    /// Создает новую попытку доставки.
    /// </summary>
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

    /// <summary>
    /// Помечает попытку как успешную.
    /// </summary>
    public void MarkDelivered()
    {
        EnsureProcessingTransition(NotificationStatus.Delivered);
        Status = NotificationStatus.Delivered;
        ErrorMessage = null;
    }

    /// <summary>
    /// Помечает попытку как неуспешную.
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        EnsureProcessingTransition(NotificationStatus.Failed);
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Помечает попытку как отправленную в dead-letter.
    /// </summary>
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
