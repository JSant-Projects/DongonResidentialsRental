using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Domain.Lease.Events;

public sealed record LeaseActivatedDomainEvent(
    LeaseId LeaseId,
    TenantId Occupancy,
    UnitId UnitId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

