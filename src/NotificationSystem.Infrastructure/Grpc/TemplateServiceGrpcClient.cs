using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.DTOs;
using NotificationSystem.Domain.Enums;
using NotificationSystem.Contracts.Grpc;

namespace NotificationSystem.Infrastructure.Grpc;

public sealed class TemplateServiceGrpcClient(NotificationSystem.Contracts.Grpc.TemplateGrpc.TemplateGrpcClient client)
    : ITemplateServiceClient
{
    public async Task<RenderedTemplateDto> RenderAsync(
        NotificationChannel channel,
        string templateCode,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var response = await client.RenderTemplateAsync(
            new RenderTemplateRequest
            {
                Channel = channel.ToString(),
                TemplateCode = templateCode,
                PayloadJson = payloadJson
            },
            cancellationToken: cancellationToken);

        return new RenderedTemplateDto(response.Subject, response.Body, response.MetadataJson);
    }
}
