using RabbitMQ.Client;

namespace NotificationSystem.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider : IDisposable
{
    IConnection GetConnection();
}
