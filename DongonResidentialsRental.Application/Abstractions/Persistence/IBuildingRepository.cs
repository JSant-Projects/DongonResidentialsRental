using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IBuildingRepository
{
    void Add(Building building);
    Task<Building?> GetByIdAsync(BuildingId building, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(BuildingId buildingId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string buildingName, CancellationToken cancellationToken = default);
    void Remove(Building building);
}
