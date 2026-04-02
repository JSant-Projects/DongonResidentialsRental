using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Domain.Unit;

public sealed record UnitId(Guid Id)
{
    public static bool TryParse(string? value, out UnitId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new UnitId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}