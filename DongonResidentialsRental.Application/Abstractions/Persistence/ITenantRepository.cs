using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task RemoveAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
