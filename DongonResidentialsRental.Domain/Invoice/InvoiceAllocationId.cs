namespace DongonResidentialsRental.Domain.Invoice;

public sealed record InvoiceAllocationId(Guid Id)
{
    public static bool TryParse(string? value, out InvoiceAllocationId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new InvoiceAllocationId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
