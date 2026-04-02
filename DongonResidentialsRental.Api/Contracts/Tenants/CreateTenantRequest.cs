namespace DongonResidentialsRental.Api.Contracts.Tenants;

public sealed record CreateTenantRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
