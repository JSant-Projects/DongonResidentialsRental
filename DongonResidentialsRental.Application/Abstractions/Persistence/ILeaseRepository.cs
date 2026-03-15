using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface ILeaseRepository
{

    void Add(Lease lease);
    Task<Lease?> GetByIdAsync(LeaseId leaseId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveLeaseForUnitAsync(
        UnitId unitId, 
        DateOnly date, 
        CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveLeaseForTenantAsync(
        TenantId tenantId, 
        DateOnly date, 
        CancellationToken cancellationToken = default);
    void Remove(Lease lease);
}
