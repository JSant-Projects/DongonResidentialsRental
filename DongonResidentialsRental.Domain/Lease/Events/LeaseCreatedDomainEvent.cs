using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Lease.Events;

public sealed record LeaseCreatedDomainEvent(
    LeaseId LeaseId,
    TenantId Occupancy,
    UnitId UnitId,
    DateOnly StartDate) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

