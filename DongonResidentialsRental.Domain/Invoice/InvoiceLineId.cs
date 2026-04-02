namespace DongonResidentialsRental.Domain.Invoice;

public sealed record InvoiceLineId(Guid Id)
{
    public static bool TryParse(string? value, out InvoiceLineId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new InvoiceLineId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
