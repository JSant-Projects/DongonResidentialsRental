namespace DongonResidentialsRental.Domain.Lease;

public sealed record UtilityResponsibility
{
    public bool TenantPaysElectricity { get; }
    public bool TenantPaysWater { get; }

    private UtilityResponsibility(bool tenantPaysElectricity, bool tenantPaysWater)
    {
        TenantPaysElectricity = tenantPaysElectricity;
        TenantPaysWater = tenantPaysWater;
    }

    public static UtilityResponsibility Create(bool tenantPaysElectricity, bool tenantPaysWater)
    {
        return new UtilityResponsibility(tenantPaysElectricity, tenantPaysWater);
    }
}
