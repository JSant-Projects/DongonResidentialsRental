using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    void Add(Tenant tenant);
    Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    void Remove(Tenant tenant);
}
