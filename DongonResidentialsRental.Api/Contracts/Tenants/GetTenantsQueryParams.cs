namespace DongonResidentialsRental.Api.Contracts.Tenants;

public sealed record GetTenantsQueryParams(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);