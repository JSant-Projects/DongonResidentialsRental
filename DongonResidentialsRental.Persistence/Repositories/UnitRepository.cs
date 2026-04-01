using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UnitRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Unit unit)
    {
        _dbContext.Units.Add(unit);
    }

    public async Task<bool> ExistsUnitNumberInBuildingAsync(BuildingId buildingId, string unitNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
                        .AnyAsync(u => 
                            u.BuildingId == buildingId && 
                            u.UnitNumber == unitNumber, 
                            cancellationToken);
    }

    public async Task<Unit?> GetByIdAsync(UnitId unitId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
                        .FirstOrDefaultAsync(u => 
                            u.UnitId == unitId, 
                            cancellationToken);
    }

    public void Remove(Unit unit)
    {
        _dbContext.Units.Remove(unit);
    }
}
