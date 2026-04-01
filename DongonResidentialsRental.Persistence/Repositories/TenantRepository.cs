using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _dbContext;
    public TenantRepository(ApplicationDbContext dbContext  )
    {
        _dbContext = dbContext;
    }

    public void Add(Tenant tenant)
    {
        _dbContext.Tenants.Add(tenant);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
                        .AnyAsync(t => 
                            t.ContactInfo.Email.Value == email, 
                            cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
                        .FirstOrDefaultAsync(t => 
                            t.TenantId == tenantId, 
                            cancellationToken);
    }

    public void Remove(Tenant tenant)
    {
        _dbContext.Tenants.Remove(tenant);
    }
}
