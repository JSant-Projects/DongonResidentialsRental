using DongonResidentialsRental.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Messaging;

internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        dynamic handler = _serviceProvider.GetRequiredService(
            typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse)));

        return await handler.Handle((dynamic)command, cancellationToken);
    }
}
