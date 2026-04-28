using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.DTOs;
using NotificationSystem.Contracts.Grpc;

namespace NotificationSystem.Infrastructure.Grpc;

public sealed class DeliveryServiceGrpcClient(NotificationSystem.Contracts.Grpc.DeliveryGrpc.DeliveryGrpcClient client)
    : IDeliveryServiceClient
{
    public async Task<DeliveryDispatchResult> SendAsync(DeliveryDispatchRequest request, CancellationToken cancellationToken)
    {
        var response = await client.SendNotificationAsync(
            new SendNotificationRequest
            {
                Channel = request.Channel.ToString(),
                Recipient = request.Recipient,
                Subject = request.Subject,
                Body = request.Body,
                CorrelationId = request.CorrelationId
            },
            cancellationToken: cancellationToken);

        return new DeliveryDispatchResult(response.IsSuccess, response.IsTransientFailure, response.ErrorMessage);
    }
}
