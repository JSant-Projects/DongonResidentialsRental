using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed record InvoiceId(Guid Id)
{
    public static bool TryParse(string? value, out InvoiceId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new InvoiceId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
