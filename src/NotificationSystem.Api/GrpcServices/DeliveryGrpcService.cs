using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using NotificationSystem.Contracts.Grpc;

namespace NotificationSystem.Api.GrpcServices;

public sealed class DeliveryGrpcService : DeliveryGrpc.DeliveryGrpcBase
{
    private static readonly ConcurrentDictionary<string, int> AttemptCounters = new();

    public override Task<SendNotificationResponse> SendNotification(SendNotificationRequest request, ServerCallContext context)
    {
        var currentAttempt = AttemptCounters.AddOrUpdate(request.CorrelationId, 1, (_, current) => current + 1);
        var recipient = request.Recipient.ToLowerInvariant();

        if (recipient.Contains("permanent"))
        {
            return Task.FromResult(new SendNotificationResponse
            {
                IsSuccess = false,
                IsTransientFailure = false,
                ErrorMessage = "Permanent delivery failure was requested by test recipient."
            });
        }

        if (recipient.Contains("transient") && currentAttempt < 3)
        {
            return Task.FromResult(new SendNotificationResponse
            {
                IsSuccess = false,
                IsTransientFailure = true,
                ErrorMessage = $"Transient delivery failure on attempt {currentAttempt}."
            });
        }

        return Task.FromResult(new SendNotificationResponse
        {
            IsSuccess = true,
            IsTransientFailure = false,
            ErrorMessage = string.Empty
        });
    }
}
