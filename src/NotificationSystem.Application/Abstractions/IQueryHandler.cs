using CSharpFunctionalExtensions;
using NotificationSystem.Contracts.Results;

namespace NotificationSystem.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse, Error>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
