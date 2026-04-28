using NotificationSystem.Application.DTOs;

namespace NotificationSystem.Application.Abstractions;

public interface IDeliveryServiceClient
{
    Task<DeliveryDispatchResult> SendAsync(DeliveryDispatchRequest request, CancellationToken cancellationToken);
}
