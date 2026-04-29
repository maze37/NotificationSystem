using NotificationSystem.Application.Abstractions;
using NotificationSystem.Contracts.Api.Notifications;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid NotificationId) : IQuery<NotificationResponse>;
