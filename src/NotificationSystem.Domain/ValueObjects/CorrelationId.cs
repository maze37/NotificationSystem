using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Constants;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Domain.ValueObjects;

/// <summary>
/// Идентификатор корреляции для идемпотентности.
/// </summary>
public sealed record CorrelationId
{
    public string Value { get; }

    private CorrelationId(string value) => Value = value;

    public static Result<CorrelationId, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CorrelationId, Error>(
                Error.Validation("correlation_id.required", "CorrelationId обязателен.", nameof(value)));
        }

        var normalized = value.Trim();
        if (normalized.Length > LengthConstants.CorrelationId)
        {
            return Result.Failure<CorrelationId, Error>(
                Error.Validation("correlation_id.length.invalid", $"CorrelationId не должен превышать {LengthConstants.CorrelationId} символов.", nameof(value)));
        }

        return Result.Success<CorrelationId, Error>(new CorrelationId(normalized));
    }
}