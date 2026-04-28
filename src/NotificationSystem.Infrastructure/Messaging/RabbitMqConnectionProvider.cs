using Microsoft.Extensions.Options;
using NotificationSystem.Infrastructure.Options;
using RabbitMQ.Client;

namespace NotificationSystem.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IRabbitMqConnectionProvider
{
    private readonly Lazy<IConnection> _connection = new(() =>
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection("NotificationSystem");
    });

    public IConnection GetConnection() => _connection.Value;

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }
}
