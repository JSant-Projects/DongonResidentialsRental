using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Messaging;

internal interface IDomainEventHandlerWrapper
{
    Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);
}