using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;
using NotificationSystem.Infrastructure.Messaging;
using NotificationSystem.Contracts.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog.Context;

namespace NotificationSystem.Worker.Services;

/// <summary>
/// Фоновый консьюмер RabbitMQ, который запускает обработку уведомлений.
/// </summary>
public sealed class RabbitMqNotificationConsumerService(
    IRabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqNotificationConsumerService> logger) : BackgroundService
{
    private IModel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private CancellationToken _stoppingToken;

    /// <summary>
    /// Поднимает подписку на очередь и держит сервис активным до остановки.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        _channel = connectionProvider.GetConnection().CreateModel();
        _channel.BasicQos(0, 1, false);

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.Received += OnReceivedAsync;

        _channel.BasicConsume(MessagingTopology.CreatedQueue, autoAck: false, _consumer);
        logger.LogInformation("Консьюмер RabbitMQ запущен. Очередь: {Queue}", MessagingTopology.CreatedQueue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Освобождает ресурсы канала и отписывается от событий.
    /// </summary>
    public override void Dispose()
    {
        if (_consumer is not null)
        {
            _consumer.Received -= OnReceivedAsync;
        }

        _channel?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Обрабатывает одно входящее сообщение из очереди.
    /// </summary>
    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        NotificationMessage? message = null;

        try
        {
            message = JsonSerializer.Deserialize<NotificationMessage>(Encoding.UTF8.GetString(args.Body.ToArray()));
            if (message is null)
            {
                logger.LogError("Пустое сообщение RabbitMQ. Сообщение будет отброшено без повтора.");
                _channel.BasicNack(args.DeliveryTag, false, requeue: false);
                return;
            }

            using var scope = scopeFactory.CreateScope();
            using (LogContext.PushProperty("CorrelationId", message.CorrelationId))
            {
                var handler = scope.ServiceProvider.GetRequiredService<ProcessNotificationCommandHandler>();
                var result = await handler.HandleAsync(
                    new ProcessNotificationCommand(message.NotificationId, message.CorrelationId),
                    _stoppingToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Обработка уведомления завершилась ошибкой. Code: {Code}, Message: {Message}, NotificationId: {NotificationId}",
                        result.Error.Code,
                        result.Error.Message,
                        message.NotificationId);
                }
            }

            _channel.BasicAck(args.DeliveryTag, false);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Некорректный JSON в сообщении RabbitMQ. Сообщение будет отброшено без повтора.");
            _channel.BasicNack(args.DeliveryTag, false, requeue: false);
        }
        catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
        {
            _channel.BasicNack(args.DeliveryTag, false, requeue: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки сообщения RabbitMQ {@Message}", message);
            _channel.BasicNack(args.DeliveryTag, false, requeue: true);
        }
    }
}
