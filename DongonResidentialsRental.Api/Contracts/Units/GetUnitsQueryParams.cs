using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Api.Contracts.Units;

public sealed record GetUnitsQueryParams(
    UnitStatus? Status,
    string? UnitNumber,
    Guid? BuildingId,
    int? Floor,
    int Page = 1,
    int PageSize = 20);
