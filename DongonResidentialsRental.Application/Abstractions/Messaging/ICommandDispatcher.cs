namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface ICommandDispatcher
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
