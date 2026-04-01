using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal class LeaseRepository : ILeaseRepository
{
    private readonly ApplicationDbContext _dbContext;
    public LeaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void Add(Lease lease)
    {
        _dbContext.Leases.Add(lease);
    }

    public async Task<bool> ExistsActiveLeaseForTenantAsync(TenantId tenantId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leases
                        .AnyAsync(l => 
                            l.Occupancy == tenantId && 
                            l.Term.StartDate <= date && 
                            l.Term.EndDate >= date && 
                            l.Status == LeaseStatus.Active, 
                            cancellationToken);
    }

    public async Task<bool> ExistsActiveLeaseForUnitAsync(UnitId unitId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leases
                        .AnyAsync(l => 
                            l.UnitId == unitId && 
                            l.Term.StartDate <= date && 
                            l.Term.EndDate >= date && 
                            l.Status == LeaseStatus.Active, 
                            cancellationToken);
    }

    public async Task<IReadOnlyList<Lease>> GetActiveLeases(DateOnly today, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leases
                        .Where(l => 
                            l.Term.StartDate <= today && 
                            (l.Term.EndDate == null || l.Term.EndDate >= today) && 
                            l.Status == LeaseStatus.Active)
                        .ToListAsync(cancellationToken);
    }

    public async Task<Lease?> GetByIdAsync(LeaseId leaseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leases
                        .FirstOrDefaultAsync(l => 
                            l.LeaseId == leaseId, 
                            cancellationToken);
    }

    public async Task<IReadOnlyList<Lease>> GetLeasesOverlappingPeriodAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        return await _dbContext.Leases
                        .Where(l =>
                            l.Term.StartDate <= end &&
                            (l.Term.EndDate == null || l.Term.EndDate >= start) &&
                            l.Status == LeaseStatus.Active)
                        .ToListAsync(cancellationToken);
    }

    public void Remove(Lease lease)
    {
        _dbContext.Leases.Remove(lease);
    }
}
