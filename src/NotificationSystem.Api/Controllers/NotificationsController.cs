using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Api.Middleware;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;
using NotificationSystem.Application.UseCases.NotificationUseCases.Queries.GetNotificationById;
using NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController(
    CreateNotificationCommandHandler createNotificationHandler,
    GetNotificationByIdQueryHandler getNotificationByIdHandler,
    SearchNotificationsQueryHandler searchNotificationsHandler) : ControllerBase
{
    private const string GetByIdRouteName = "Notifications.GetById";

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationChannel>(request.Channel, true, out var channel))
        {
            return BadRequest(Error.Validation("notification.channel.invalid", "Некорректный канал уведомления.", nameof(request.Channel)));
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
            return ToProblem(result.Error);
        }

        var payload = result.Value;
        if (payload.IsDuplicate)
        {
            return Ok(payload);
        }

        return CreatedAtRoute(GetByIdRouteName, new { id = payload.Notification.Id }, payload);
    }

    [HttpGet("{id:guid}", Name = GetByIdRouteName)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await getNotificationByIdHandler.HandleAsync(new GetNotificationByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return ToProblem(result.Error);
        }

        return Ok(result.Value);
    }

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
            return ToProblem(result.Error);
        }

        return Ok(result.Value);
    }

    private IActionResult ToProblem(Error error)
    {
        return error.Type switch
        {
            "validation" => BadRequest(error),
            "not_found" => NotFound(error),
            "conflict" => Conflict(error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, error)
        };
    }
}
