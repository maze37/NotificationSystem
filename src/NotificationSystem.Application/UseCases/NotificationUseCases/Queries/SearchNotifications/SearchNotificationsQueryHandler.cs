using CSharpFunctionalExtensions;
using FluentValidation;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Application.Common;
using NotificationSystem.Contracts.Api.Notifications;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;

public sealed class SearchNotificationsQueryHandler : IQueryHandler<SearchNotificationsQuery, IReadOnlyCollection<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IValidator<SearchNotificationsQuery> _validator;

    public SearchNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        IValidator<SearchNotificationsQuery> validator)
    {
        _notificationRepository = notificationRepository;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyCollection<NotificationResponse>, Error>> HandleAsync(
        SearchNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationError<IReadOnlyCollection<NotificationResponse>>();
        }

        var items = await _notificationRepository.SearchAsync(
            query.Status,
            query.Channel,
            query.FromUtc,
            query.ToUtc,
            cancellationToken);

        return Result.Success<IReadOnlyCollection<NotificationResponse>, Error>(
            items.Select(static x => x.ToResponse()).ToArray());
    }
}