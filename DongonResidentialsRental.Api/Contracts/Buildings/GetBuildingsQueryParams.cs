using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Api.Contracts.Buildings;

public sealed record GetBuildingsQueryParams(
    BuildingStatus? Status,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);
