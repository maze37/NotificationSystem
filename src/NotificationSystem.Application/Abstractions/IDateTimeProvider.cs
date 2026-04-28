namespace NotificationSystem.Application.Abstractions;

/// <summary>
/// Провайдер времени для Application-слоя.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Текущее время в UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
