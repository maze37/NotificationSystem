namespace NotificationSystem.Contracts.Results;

/// <summary>
/// Унифицированный HTTP-ответ для успешных и ошибочных сценариев.
/// </summary>
public sealed record Envelope<T>(T? Result, IReadOnlyCollection<Error>? Errors, DateTimeOffset TimeGenerated)
{
    public static Envelope<T> Success(T result) =>
        new(result, null, DateTimeOffset.UtcNow);

    public static Envelope<T> Failure(params IReadOnlyCollection<Error> errors) =>
        new(default, errors, DateTimeOffset.UtcNow);

    public static Envelope<T> Failure(params Error[] errors) =>
        new(default, errors, DateTimeOffset.UtcNow);
}
