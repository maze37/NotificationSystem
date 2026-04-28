using NotificationSystem.Domain.Exceptions;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Идентификатор корреляции для идемпотентности.
/// </summary>
public sealed record CorrelationId
{
    public string Value { get; }

    private CorrelationId(string value) => Value = value;

    public static CorrelationId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("CorrelationId обязателен.");
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.CorrelationId)
        {
            throw new DomainRuleException($"CorrelationId не должен превышать {LengthConstants.CorrelationId} символов.");
        }

        return new CorrelationId(normalized);
    }
}
