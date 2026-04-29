using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Constants;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Получатель уведомления.
/// </summary>
public sealed record NotificationRecipient
{
    public string Value { get; }

    private NotificationRecipient(string value) => Value = value;

    public static Result<NotificationRecipient, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<NotificationRecipient, Error>(
                Error.Validation("notification_recipient.required", "Получатель обязателен.", nameof(value)));
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.Recipient)
        {
            return Result.Failure<NotificationRecipient, Error>(
                Error.Validation("notification_recipient.length.invalid", $"Получатель не должен превышать {LengthConstants.Recipient} символов.", nameof(value)));
        }

        return Result.Success<NotificationRecipient, Error>(new NotificationRecipient(normalized));
    }
}