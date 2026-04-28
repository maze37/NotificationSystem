using NotificationSystem.Application.Abstractions;

namespace NotificationSystem.Infrastructure.Services;

/// <summary>
/// Системная реализация провайдера времени.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
