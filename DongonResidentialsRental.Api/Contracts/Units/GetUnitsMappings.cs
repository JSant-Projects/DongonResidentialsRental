using DongonResidentialsRental.Application.Parsing;
using DongonResidentialsRental.Application.Units.Queries.GetUnits;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Api.Contracts.Units;

public static class GetUnitsMappings
{
    public static GetUnitsQuery ToQuery(this GetUnitsQueryParams queryParams)
    {
        var buildingId = queryParams.BuildingId.HasValue ?
                           new BuildingId(queryParams.BuildingId.Value) :
                           null;

        return new GetUnitsQuery(
            EnumQueryParser.ParseNullable<UnitStatus>(queryParams.Status),
            queryParams.UnitNumber,
            buildingId,
            queryParams.Floor,
            queryParams.Page,
            queryParams.PageSize);
    }
}
