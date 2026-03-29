using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IBuildingRepository
{
    void Add(Building building);
    Task<BuildingId?> GetByIdAsync(BuildingId building, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(BuildingId buildingId, CancellationToken cancellationToken = default);
    void Remove(Building building);
}
