using FluentValidation;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.ProcessNotification;

public sealed class ProcessNotificationCommandValidator : AbstractValidator<ProcessNotificationCommand>
{
    public ProcessNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
    }
}
