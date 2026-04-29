using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Constants;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Код шаблона уведомления.
/// </summary>
public sealed record TemplateCode
{
    public string Value { get; }

    private TemplateCode(string value) => Value = value;

    public static Result<TemplateCode, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<TemplateCode, Error>(
                Error.Validation("template_code.required", "Код шаблона обязателен.", nameof(value)));
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.TemplateCode)
        {
            return Result.Failure<TemplateCode, Error>(
                Error.Validation("template_code.length.invalid", $"Код шаблона не должен превышать {LengthConstants.TemplateCode} символов.", nameof(value)));
        }

        return Result.Success<TemplateCode, Error>(new TemplateCode(normalized));
    }
}