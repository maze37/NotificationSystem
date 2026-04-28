namespace NotificationSystem.Contracts.Results;

/// <summary>
/// Стандартизированная ошибка приложения.
/// </summary>
public sealed record Error(string Code, string Message, string Type, string? Field = null)
{
    public static Error Validation(string code, string message, string? field = null) =>
        new(code, message, "validation", field);

    public static Error NotFound(string code, string message) =>
        new(code, message, "not_found");

    public static Error Conflict(string code, string message) =>
        new(code, message, "conflict");

    public static Error Failure(string code, string message) =>
        new(code, message, "failure");
}
