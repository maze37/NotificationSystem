using FluentValidation;
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
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Ошибка валидации",
                errors = ex.Errors.Select(x => new { x.PropertyName, x.ErrorMessage })
            });
        }
        catch (DomainRuleException ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Нарушение доменного правила",
                detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необработанное исключение");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Внутренняя ошибка сервера",
                detail = "Подробности доступны в логах."
            });
        }
    }
}
