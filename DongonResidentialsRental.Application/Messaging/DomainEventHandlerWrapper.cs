using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Messaging;

internal sealed class DomainEventHandlerWrapper<TDomainEvent> : 
    IDomainEventHandlerWrapper
    where TDomainEvent : IDomainEvent
{
    private readonly IEnumerable<IDomainEventHandler<TDomainEvent>> _handlers;

    public DomainEventHandlerWrapper(IEnumerable<IDomainEventHandler<TDomainEvent>> handlers)
    {
        _handlers = handlers;
    }

    public async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        TDomainEvent typedDomainEvent = (TDomainEvent)domainEvent;

        foreach (IDomainEventHandler<TDomainEvent> handler in _handlers)
        {
            await handler.Handle(typedDomainEvent, cancellationToken);
        }
    }
}