using DongonResidentialsRental.Application.Buildings.Queries;
using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Application.Buildings;

public static class BuildingMappings
{
    public static BuildingResponse ToResponse(this Building building)
    {
        return new BuildingResponse(
            building.BuildingId.Id,
            building.Name,
            building.Address.Street,
            building.Address.City,
            building.Address.Province,
            building.Address.PostalCode);
    }

    public static BuildingLookupResponse ToLookupResponse(this Building building)
    {
        return new BuildingLookupResponse(building.BuildingId.Id, 
            building.Name);
    }
}
