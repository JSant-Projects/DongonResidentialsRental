using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Domain.Lease.Events;

public sealed record LeaseTerminatedDomainEvent(
    LeaseId LeaseId,
    TenantId Occupancy,
    UnitId UnitId,
    DateOnly TerminationDate) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

