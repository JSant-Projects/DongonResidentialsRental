using DongonResidentialsRental.Application.Buildings.Queries;
using DongonResidentialsRental.Domain.Building;
using System.Linq.Expressions;

namespace DongonResidentialsRental.Application.Buildings;

public static class BuildingMappings
{
    public static Expression<Func<Building, BuildingResponse>> ToResponse()
        => building => new BuildingResponse(
            building.BuildingId.Id,
            building.Name,
            building.Address.Street,
            building.Address.City,
            building.Address.Province,
            building.Address.PostalCode);

    public static Expression<Func<Building, BuildingLookupResponse>> ToLookupResponse()
    {
        return building => new BuildingLookupResponse(
            building.BuildingId.Id,
            building.Name);
    }
}
