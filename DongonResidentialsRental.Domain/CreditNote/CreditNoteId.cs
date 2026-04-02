using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Domain.CreditNote;

public sealed record CreditNoteId(Guid Id)
{
    public static bool TryParse(string? value, out CreditNoteId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new CreditNoteId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
