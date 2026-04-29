using NotificationSystem.Application.Abstractions;
using CSharpFunctionalExtensions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler : IQueryHandler<GetNotificationByIdQuery, NotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationByIdQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<NotificationResponse, Error>> HandleAsync(GetNotificationByIdQuery query, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(query.NotificationId, cancellationToken);
        if (notification is null)
        {
            return Result.Failure<NotificationResponse, Error>(
                Error.NotFound("notification.not_found", "Уведомление не найдено."));
        }

        return Result.Success<NotificationResponse, Error>(notification.ToResponse());
    }
}