using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IUnitRepository
{
    void Add(Unit unit);
    Task<Unit?> GetByIdAsync(UnitId unitId, CancellationToken cancellationToken = default);
    void Remove(Unit unit);
}
