using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IUnitRepository
{
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(UnitId unitId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Unit unit, CancellationToken cancellationToken = default);
}
