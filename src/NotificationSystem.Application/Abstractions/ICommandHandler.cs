using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.Abstractions;

/// <summary>
/// Контракт обработчика командного сценария (изменяет состояние системы).
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Выполняет команду и возвращает результат с ошибкой бизнес-уровня при неуспехе.
    /// </summary>
    Task<Result<TResponse, Error>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
