namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record GetLeasesQueryParams(
    string? SearchTerm,
    Guid? BuildingId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);
