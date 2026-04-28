namespace NotificationSystem.Infrastructure.Options;

public sealed class GrpcEndpointsOptions
{
    public const string SectionName = "GrpcEndpoints";

    public string TemplateServiceUrl { get; init; } = "http://localhost:8080";

    public string DeliveryServiceUrl { get; init; } = "http://localhost:8080";
}
