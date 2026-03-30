namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface IQueryDispatcher
{
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
