using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Domain.Meter;

public sealed record MeterId(Guid Id)
{
    public static bool TryParse(string? value, out MeterId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new MeterId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
