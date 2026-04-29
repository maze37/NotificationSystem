namespace NotificationSystem.Contracts.Results;

/// <summary>
/// Унифицированный HTTP-ответ для успешных и ошибочных сценариев.
/// </summary>
public sealed record Envelope<T>(T? Result, IReadOnlyCollection<Error>? Errors, DateTimeOffset TimeGenerated)
{
    /// <summary>
    /// Создает успешный ответ с данными.
    /// </summary>
    public static Envelope<T> Success(T result) =>
        new(result, null, DateTimeOffset.UtcNow);

    /// <summary>
    /// Создает ошибочный ответ с коллекцией ошибок.
    /// </summary>
    public static Envelope<T> Failure(params IReadOnlyCollection<Error> errors) =>
        new(default, errors, DateTimeOffset.UtcNow);

    /// <summary>
    /// Создает ошибочный ответ с набором ошибок.
    /// </summary>
    public static Envelope<T> Failure(params Error[] errors) =>
        new(default, errors, DateTimeOffset.UtcNow);
}
