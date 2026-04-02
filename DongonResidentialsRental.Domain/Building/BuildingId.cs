using DongonResidentialsRental.Domain.CreditNote;

namespace DongonResidentialsRental.Domain.Building;

public sealed record BuildingId(Guid Id)
{
    public static bool TryParse(string? value, out BuildingId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new BuildingId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}