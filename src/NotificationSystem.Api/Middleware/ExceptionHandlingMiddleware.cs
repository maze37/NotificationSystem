using FluentValidation;
using NotificationSystem.Contracts.Results;
using NotificationSystem.Domain.Exceptions;

namespace NotificationSystem.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = ex.Errors
                .Select(static x => Error.Validation("validation.failed", x.ErrorMessage, x.PropertyName))
                .ToArray();
            await context.Response.WriteAsJsonAsync(Envelope<object>.Failure(errors));
        }
        catch (DomainRuleException ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(
                Envelope<object>.Failure(Error.Conflict("domain.rule_violation", ex.Message)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необработанное исключение");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(
                Envelope<object>.Failure(Error.Failure("server.internal", "Внутренняя ошибка сервера. Подробности доступны в логах.")));
        }
    }
}
