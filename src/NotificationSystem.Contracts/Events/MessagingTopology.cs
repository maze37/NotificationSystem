namespace NotificationSystem.Contracts.Events;

public static class MessagingTopology
{
    public const string ExchangeName = "notifications.exchange";
    public const string DeadLetterExchangeName = "notifications.dead-letter.exchange";
    public const string CreatedRoutingKey = "notifications.created";
    public const string DeadLetterRoutingKey = "notifications.dead-letter";
    public const string CreatedQueue = "notifications.created.queue";
    public const string DeadLetterQueue = "notifications.dead-letter.queue";

    public static readonly IReadOnlyList<(string QueueName, string RoutingKey, int TtlMilliseconds)> RetryQueues =
    [
        ("notifications.retry.1.queue", "notifications.retry.1", 10_000),
        ("notifications.retry.2.queue", "notifications.retry.2", 30_000),
        ("notifications.retry.3.queue", "notifications.retry.3", 60_000),
        ("notifications.retry.4.queue", "notifications.retry.4", 300_000)
    ];
}
