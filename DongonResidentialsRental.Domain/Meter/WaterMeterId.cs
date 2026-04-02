using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Domain.Meter;

public sealed record WaterMeterId(Guid Id)
{
    public static bool TryParse(string? value, out WaterMeterId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new WaterMeterId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
