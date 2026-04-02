using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Domain.Payment;

public sealed record PaymentId(Guid Id)
{
    public static bool TryParse(string? value, out PaymentId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new PaymentId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
