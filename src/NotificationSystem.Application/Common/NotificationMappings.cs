using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common;

public static class NotificationMappings
{
    public static NotificationResponse ToResponse(this NotificationJob job) =>
        new(
            job.Id,
            job.Channel.ToString(),
            job.Recipient,
            job.TemplateCode,
            job.PayloadJson,
            job.Status.ToString(),
            job.Attempts,
            job.CreatedWhen,
            job.UpdatedWhen,
            job.ErrorMessage,
            job.CorrelationId);

    public static DeliveryAttemptResponse ToResponse(this DeliveryAttempt attempt) =>
        new(
            attempt.Id,
            attempt.NotificationId,
            attempt.AttemptNumber,
            attempt.Status.ToString(),
            attempt.ErrorMessage,
            attempt.CreatedWhen);
}
