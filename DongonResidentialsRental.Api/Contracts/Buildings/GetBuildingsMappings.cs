using DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;
using DongonResidentialsRental.Application.Parsing;
using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Api.Contracts.Buildings;

public static class GetBuildingsMappings
{
    public static GetBuildingsQuery ToQuery(this GetBuildingsQueryParams queryParams)
    {
        return new GetBuildingsQuery(
            EnumQueryParser.ParseNullable<BuildingStatus>(queryParams.Status),
            queryParams.SearchTerm,
            queryParams.Page,
            queryParams.PageSize);
    }
}
