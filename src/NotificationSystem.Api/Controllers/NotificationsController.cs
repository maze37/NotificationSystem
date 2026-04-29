using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Api.Middleware;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;
using NotificationSystem.Application.UseCases.NotificationUseCases.Queries.GetNotificationById;
using NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Api.Controllers;

/// <summary>
/// HTTP-точки для создания и чтения уведомлений.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(
    ICommandHandler<CreateNotificationCommand, CreateNotificationResponse> createNotificationHandler,
    IQueryHandler<GetNotificationByIdQuery, NotificationResponse> getNotificationByIdHandler,
    IQueryHandler<SearchNotificationsQuery, IReadOnlyCollection<NotificationResponse>> searchNotificationsHandler) : ControllerBase
{
    private const string GetByIdRouteName = "Notifications.GetById";

    /// <summary>
    /// Создает новое уведомление и ставит его в обработку.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationChannel>(request.Channel, true, out var channel))
        {
            return this.ToErrorResponse(Error.Validation("notification.channel.invalid", "Некорректный канал уведомления.", nameof(request.Channel)));
        }

        var correlationId = request.CorrelationId
            ?? HttpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString()
            ?? Guid.NewGuid().ToString("N");

        var result = await createNotificationHandler.HandleAsync(
            new CreateNotificationCommand(
                channel,
                request.Recipient,
                request.TemplateCode,
                request.Payload.GetRawText(),
                correlationId),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToErrorResponse(result.Error);
        }

        var payload = result.Value;
        if (payload.IsDuplicate)
        {
            return Ok(Envelope<CreateNotificationResponse>.Success(payload));
        }

        return CreatedAtRoute(GetByIdRouteName, new { id = payload.Notification.Id }, Envelope<CreateNotificationResponse>.Success(payload));
    }

    /// <summary>
    /// Возвращает уведомление по идентификатору.
    /// </summary>
    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await getNotificationByIdHandler.HandleAsync(new GetNotificationByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return this.ToErrorResponse(result.Error);
        }

        return Ok(Envelope<NotificationResponse>.Success(result.Value));
    }

    /// <summary>
    /// Ищет уведомления по статусу, каналу и диапазону дат.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] NotificationStatus? status,
        [FromQuery] NotificationChannel? channel,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var result = await searchNotificationsHandler.HandleAsync(
            new SearchNotificationsQuery(status, channel, from, to),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToErrorResponse(result.Error);
        }

        return Ok(Envelope<IReadOnlyCollection<NotificationResponse>>.Success(result.Value));
    }
}
