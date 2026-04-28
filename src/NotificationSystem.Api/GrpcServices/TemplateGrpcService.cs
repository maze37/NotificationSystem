using System.Text.Json;
using Grpc.Core;
using NotificationSystem.Contracts.Grpc;

namespace NotificationSystem.Api.GrpcServices;

public sealed class TemplateGrpcService : TemplateGrpc.TemplateGrpcBase
{
    public override Task<RenderTemplateResponse> RenderTemplate(RenderTemplateRequest request, ServerCallContext context)
    {
        using var document = JsonDocument.Parse(request.PayloadJson);
        var root = document.RootElement;
        var user = root.TryGetProperty("name", out var name) ? name.GetString() : request.RecipientFallback();
        var content = root.TryGetProperty("message", out var message) ? message.GetString() : request.PayloadJson;

        return Task.FromResult(new RenderTemplateResponse
        {
            Subject = $"{request.TemplateCode} for {user}",
            Body = $"[{request.Channel}] {content}",
            MetadataJson = "{\"renderer\":\"mock-template-service\"}"
        });
    }
}

internal static class RenderTemplateRequestExtensions
{
    public static string RecipientFallback(this RenderTemplateRequest request) =>
        request.Channel.Equals("Webhook", StringComparison.OrdinalIgnoreCase) ? "webhook-target" : "recipient";
}
