namespace NotificationSystem.Infrastructure.Options;

/// <summary>
/// Параметры подключения к RabbitMQ и стратегия задержек retry.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Хост RabbitMQ.
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// Порт RabbitMQ.
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Пользователь для подключения.
    /// </summary>
    public string UserName { get; init; } = "guest";

    /// <summary>
    /// Пароль для подключения.
    /// </summary>
    public string Password { get; init; } = "guest";

    /// <summary>
    /// Virtual host в RabbitMQ.
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Задержки между повторными попытками в секундах.
    /// </summary>
    public IReadOnlyList<int> RetryDelaysSeconds { get; init; } = [10, 30, 60, 300];
}
