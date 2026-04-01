using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class BuildingRepository : IBuildingRepository
{
    private readonly ApplicationDbContext _dbContext;
    public BuildingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void Add(Building building)
    {
        _dbContext.Buildings.Add(building);
    }

    public async Task<bool> ExistsAsync(BuildingId buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Buildings
                        .AnyAsync(
                            b => b.BuildingId == buildingId, 
                            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string buildingName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Buildings
                        .AnyAsync(
                            b => b.Name == buildingName, 
                            cancellationToken);
    }

    public async Task<Building?> GetByIdAsync(BuildingId building, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Buildings
                        .FirstOrDefaultAsync(
                            b => b.BuildingId == building, 
                            cancellationToken);
    }

    public void Remove(Building building)
    {
        _dbContext.Buildings.Remove(building);
    }
}
