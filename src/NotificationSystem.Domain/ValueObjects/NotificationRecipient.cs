using NotificationSystem.Domain.Exceptions;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Получатель уведомления.
/// </summary>
public sealed record NotificationRecipient
{
    public string Value { get; }

    private NotificationRecipient(string value) => Value = value;

    public static NotificationRecipient Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Получатель обязателен.");
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.Recipient)
        {
            throw new DomainRuleException($"Получатель не должен превышать {LengthConstants.Recipient} символов.");
        }

        return new NotificationRecipient(normalized);
    }
}
