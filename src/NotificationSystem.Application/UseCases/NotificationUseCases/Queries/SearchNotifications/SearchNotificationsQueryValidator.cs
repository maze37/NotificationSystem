using FluentValidation;
using NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Queries.SearchNotifications;

public sealed class SearchNotificationsQueryValidator : AbstractValidator<SearchNotificationsQuery>
{
    public SearchNotificationsQueryValidator()
    {
        RuleFor(x => x).Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc <= x.ToUtc)
            .WithMessage("Параметр 'from' должен быть меньше или равен параметру 'to'.");
    }
}
