using CSharpFunctionalExtensions;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler(INotificationRepository notificationRepository)
{
    public async Task<Result<NotificationResponse, Error>> HandleAsync(GetNotificationByIdQuery query, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(query.NotificationId, cancellationToken);
        if (notification is null)
        {
            return Result.Failure<NotificationResponse, Error>(
                Error.NotFound("notification.not_found", "Уведомление не найдено."));
        }

        return Result.Success<NotificationResponse, Error>(notification.ToResponse());
    }
}
