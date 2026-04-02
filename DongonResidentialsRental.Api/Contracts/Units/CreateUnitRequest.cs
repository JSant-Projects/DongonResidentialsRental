using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Api.Contracts.Units;

public sealed record CreateUnitRequest(
    Guid BuildingId,
    string UnitNumber,
    int? Floor);
