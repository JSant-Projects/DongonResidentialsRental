using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Domain.Lease;

public sealed record LeaseId(Guid Id)
{
    public static bool TryParse(string? value, out LeaseId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new LeaseId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
