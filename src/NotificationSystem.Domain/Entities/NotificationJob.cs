using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Enums;
using NotificationSystem.Domain.Exceptions;
using NotificationSystem.Domain.ValueObjects;
using NotificationSystem.Contracts.Base;
using CorrelationIdValueObject = NotificationSystem.Domain.ValueObjects.CorrelationId;
using TemplateCodeValueObject = NotificationSystem.Domain.ValueObjects.TemplateCode;

namespace NotificationSystem.Domain.Entities;

/// <summary>
/// Доменная сущность задания на отправку уведомления.
/// </summary>
public sealed class NotificationJob : AggregateRoot
{
    public const int DefaultMaxAttempts = 5;

    private NotificationJob()
    {
    }

    private NotificationJob(
        Guid id,
        NotificationChannel channel,
        string recipient,
        string templateCode,
        string payloadJson,
        string correlationId,
        DateTimeOffset createdWhen) : base(id)
    {
        Channel = channel;
        Recipient = recipient;
        TemplateCode = templateCode;
        PayloadJson = payloadJson;
        CorrelationId = correlationId;
        Status = NotificationStatus.Created;
        CreatedWhen = createdWhen;
        UpdatedWhen = createdWhen;
    }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string TemplateCode { get; private set; } = string.Empty;

    public string PayloadJson { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset CreatedWhen { get; private set; }

    public DateTimeOffset UpdatedWhen { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    public ICollection<DeliveryAttempt> DeliveryAttempts { get; private set; } = new List<DeliveryAttempt>();

    public static Result<NotificationJob, Error> Create(
        Guid id,
        NotificationChannel channel,
        string recipient,
        string templateCode,
        string payloadJson,
        string correlationId,
        DateTimeOffset createdWhen)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<NotificationJob, Error>(
                Error.Validation("notification.id.invalid", "Идентификатор уведомления не может быть пустым.", nameof(id)));
        }

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return Result.Failure<NotificationJob, Error>(
                Error.Validation("notification.payload.required", "Payload JSON обязателен.", nameof(payloadJson)));
        }
        try
        {
            var recipientValue = NotificationRecipient.Create(recipient);
            var templateCodeValue = TemplateCodeValueObject.Create(templateCode);
            var correlationIdValue = CorrelationIdValueObject.Create(correlationId);

            return Result.Success<NotificationJob, Error>(new NotificationJob(
                id,
                channel,
                recipientValue.Value.Value,
                templateCodeValue.Value.Value,
                payloadJson,
                correlationIdValue.Value.Value,
                createdWhen));
        }
        catch (DomainRuleException ex)
        {
            return Result.Failure<NotificationJob, Error>(
                Error.Validation("notification.domain_rule", ex.Message));
        }
    }

    public void MarkQueued(DateTimeOffset updatedWhen)
    {
        EnsureTransition(NotificationStatus.Created, NotificationStatus.Queued);
        Status = NotificationStatus.Queued;
        UpdatedWhen = updatedWhen;
    }

    public int StartProcessing(DateTimeOffset updatedWhen, int maxAttempts = DefaultMaxAttempts)
    {
        if (maxAttempts <= 0)
        {
            throw new DomainRuleException("Максимум попыток должен быть больше нуля.");
        }

        if (Status is NotificationStatus.Delivered or NotificationStatus.DeadLettered)
        {
            throw new DomainRuleException($"Уведомление {Id} уже находится в финальном статусе {Status}.");
        }

        if (Attempts >= maxAttempts)
        {
            throw new DomainRuleException($"Уведомление {Id} достигло лимита попыток {maxAttempts}.");
        }

        if (Status is not NotificationStatus.Queued and not NotificationStatus.Failed and not NotificationStatus.Created)
        {
            throw new DomainRuleException($"Нельзя перевести уведомление {Id} из {Status} в Processing.");
        }

        Attempts++;
        Status = NotificationStatus.Processing;
        ErrorMessage = null;
        UpdatedWhen = updatedWhen;
        return Attempts;
    }

    public void MarkDelivered(DateTimeOffset updatedWhen)
    {
        EnsureTransition(NotificationStatus.Processing, NotificationStatus.Delivered);
        Status = NotificationStatus.Delivered;
        ErrorMessage = null;
        UpdatedWhen = updatedWhen;
    }

    public void MarkFailed(string errorMessage, DateTimeOffset updatedWhen)
    {
        EnsureTransition(NotificationStatus.Processing, NotificationStatus.Failed);
        Status = NotificationStatus.Failed;
        ErrorMessage = NormalizeError(errorMessage);
        UpdatedWhen = updatedWhen;
    }

    public void MarkDeadLettered(string errorMessage, DateTimeOffset updatedWhen)
    {
        if (Status != NotificationStatus.Processing && !IsRetryExhausted(DefaultMaxAttempts))
        {
            throw new DomainRuleException($"Нельзя перевести уведомление {Id} из {Status} в DeadLettered.");
        }

        Status = NotificationStatus.DeadLettered;
        ErrorMessage = NormalizeError(errorMessage);
        UpdatedWhen = updatedWhen;
    }

    public bool IsRetryExhausted(int maxAttempts = DefaultMaxAttempts) => Attempts >= maxAttempts;

    public bool CanRetry(int maxAttempts = DefaultMaxAttempts) =>
        Status is NotificationStatus.Failed && Attempts < maxAttempts;

    private void EnsureTransition(NotificationStatus expectedCurrent, NotificationStatus next)
    {
        if (Status != expectedCurrent)
        {
            throw new DomainRuleException($"Нельзя перевести уведомление {Id} из {Status} в {next}.");
        }
    }

    private static string NormalizeError(string? errorMessage) =>
        string.IsNullOrWhiteSpace(errorMessage)
            ? "Неизвестная ошибка доставки."
            : errorMessage.Trim();
}
