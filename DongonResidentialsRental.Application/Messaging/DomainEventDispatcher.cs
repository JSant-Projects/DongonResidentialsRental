using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Messaging;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent); 

        Type wrapperType = typeof(DomainEventHandlerWrapper<>).MakeGenericType(domainEvent.GetType());

        var wrapper = (IDomainEventHandlerWrapper)ActivatorUtilities.CreateInstance(
            _serviceProvider,
            wrapperType);

        await wrapper.Handle(domainEvent, cancellationToken);
    }

    public async Task Publish(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await Publish(domainEvent, cancellationToken);
        }
    }
}
