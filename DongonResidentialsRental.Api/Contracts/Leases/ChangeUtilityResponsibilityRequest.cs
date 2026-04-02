namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record ChangeUtilityResponsibilityRequest(
    bool TenantPaysElectricity,
    bool TenantPaysWater);
