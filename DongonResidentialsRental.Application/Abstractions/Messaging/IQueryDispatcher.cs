namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface IQueryDispatcher
{
    Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
