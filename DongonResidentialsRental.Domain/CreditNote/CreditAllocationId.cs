namespace DongonResidentialsRental.Domain.CreditNote;

public sealed record CreditAllocationId(Guid Id)
{
    public static bool TryParse(string? value, out CreditAllocationId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new CreditAllocationId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
