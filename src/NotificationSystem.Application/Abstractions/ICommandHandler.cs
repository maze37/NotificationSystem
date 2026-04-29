using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.Abstractions;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse, Error>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
