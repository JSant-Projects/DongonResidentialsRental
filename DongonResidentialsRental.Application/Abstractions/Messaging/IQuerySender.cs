namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface IQuerySender
{
    Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
