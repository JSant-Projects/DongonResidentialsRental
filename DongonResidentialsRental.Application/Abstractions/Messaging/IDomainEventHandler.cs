using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Messaging;


public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
