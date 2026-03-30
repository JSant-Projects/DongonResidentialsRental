using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task Publish(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
