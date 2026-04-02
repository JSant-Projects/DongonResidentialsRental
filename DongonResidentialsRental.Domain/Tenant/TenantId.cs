using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Domain.Tenant;

public sealed record TenantId(Guid Id)
{
    public static bool TryParse(string? value, out TenantId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new TenantId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}