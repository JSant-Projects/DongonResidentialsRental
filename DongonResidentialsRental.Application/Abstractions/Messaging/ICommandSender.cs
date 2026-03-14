namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface ICommandSender
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
