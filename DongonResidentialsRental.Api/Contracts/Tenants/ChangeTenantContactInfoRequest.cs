using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Contracts.Tenants;

public sealed record ChangeTenantContactInfoRequest(
    string Email,
    string PhoneNumber);
