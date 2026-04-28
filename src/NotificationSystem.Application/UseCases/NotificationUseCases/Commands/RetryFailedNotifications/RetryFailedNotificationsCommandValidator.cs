using FluentValidation;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.RetryFailedNotifications;

public sealed class RetryFailedNotificationsCommandValidator : AbstractValidator<RetryFailedNotificationsCommand>
{
    public RetryFailedNotificationsCommandValidator()
    {
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 1000);
        RuleFor(x => x.OlderThanMinutes).InclusiveBetween(1, 1440);
    }
}
