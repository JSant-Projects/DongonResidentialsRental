using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Domain.Payment;

public sealed record PaymentAllocationId(Guid Id)
{
    public static bool TryParse(string? value, out PaymentAllocationId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new PaymentAllocationId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
