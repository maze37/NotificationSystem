using FluentValidation;
using NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Application.UseCases.NotificationUseCases.Commands.CreateNotification;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.Recipient).NotEmpty().MaximumLength(LengthConstants.Recipient);
        RuleFor(x => x.TemplateCode).NotEmpty().MaximumLength(LengthConstants.TemplateCode);
        RuleFor(x => x.PayloadJson).NotEmpty().Must(BeValidJson).WithMessage("PayloadJson должен быть корректным JSON-документом.");
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(LengthConstants.CorrelationId);
    }

    private static bool BeValidJson(string payloadJson)
    {
        try
        {
            _ = System.Text.Json.JsonDocument.Parse(payloadJson);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
