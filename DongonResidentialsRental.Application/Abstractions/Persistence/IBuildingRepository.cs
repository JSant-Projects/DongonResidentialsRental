using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IBuildingRepository
{
    Task AddAsync(Building building, CancellationToken cancellationToken = default);
    Task<BuildingId?> GetByIdAsync(BuildingId building, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(BuildingId buildingId, CancellationToken cancellationToken);
    Task RemoveAsync(Building building, CancellationToken cancellationToken = default);
}
