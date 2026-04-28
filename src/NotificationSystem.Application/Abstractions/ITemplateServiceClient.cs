using NotificationSystem.Application.DTOs;
using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.Abstractions;

public interface ITemplateServiceClient
{
    Task<RenderedTemplateDto> RenderAsync(
        NotificationChannel channel,
        string templateCode,
        string payloadJson,
        CancellationToken cancellationToken);
}
