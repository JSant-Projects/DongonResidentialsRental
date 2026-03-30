using DongonResidentialsRental.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Messaging;

internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        dynamic handler = _serviceProvider.GetRequiredService(
            typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse)));

        return await handler.Handle((dynamic)query, cancellationToken);
    }
}
