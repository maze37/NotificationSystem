using NotificationSystem.Domain.Exceptions;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Код шаблона уведомления.
/// </summary>
public sealed record TemplateCode
{
    public string Value { get; }

    private TemplateCode(string value) => Value = value;

    public static TemplateCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException("Код шаблона обязателен.");
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.TemplateCode)
        {
            throw new DomainRuleException($"Код шаблона не должен превышать {LengthConstants.TemplateCode} символов.");
        }

        return new TemplateCode(normalized);
    }
}
