namespace DongonResidentialsRental.Api.Contracts.Tenants;

public sealed record ChangeTenantNameRequest(
    string FirstName,
    string LastName);
