using CSharpFunctionalExtensions;
using FluentValidation.Results;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.Common;

/// <summary>
/// Вспомогательные методы для преобразования ошибок валидации в Error.
/// </summary>
public static class ValidationExtensions
{
    public static Result<T, Error> ToValidationError<T>(this ValidationResult validationResult)
    {
        var firstError = validationResult.Errors.FirstOrDefault();
        if (firstError is null)
        {
            return Result.Failure<T, Error>(Error.Validation("validation.failed", "Ошибка валидации."));
        }

        return Result.Failure<T, Error>(
            Error.Validation("validation.failed", firstError.ErrorMessage, firstError.PropertyName));
    }
}
