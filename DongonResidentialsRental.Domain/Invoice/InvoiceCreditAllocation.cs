using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class InvoiceCreditAllocation
{
    public InvoiceCreditAllocationId InvoiceCreditAllocationId { get; }
    public InvoiceId InvoiceId { get; }
    public CreditNoteId CreditNoteId { get; }
    public Money Amount { get; }
    public DateOnly AppliedOn { get; }

    private InvoiceCreditAllocation() { }

    private InvoiceCreditAllocation(
        InvoiceId invoiceId, 
        CreditNoteId creditNoteId,
        Money amount,
        DateOnly appliedOn)
    {
        InvoiceCreditAllocationId = new InvoiceCreditAllocationId(Guid.NewGuid());
        InvoiceId = invoiceId;
        CreditNoteId = creditNoteId;
        Amount = amount;
        AppliedOn = appliedOn;
    }

    internal static InvoiceCreditAllocation Create(
        InvoiceId invoiceId,
        CreditNoteId creditNoteId,
        Money amount,
        DateOnly appliedOn)
    {
        Ensure.NotNull(creditNoteId);
        Ensure.NotNull(amount);
        Ensure.NonNegativeDecimal(amount.Amount, "Allocation amount must be greater than zero.");

        return new InvoiceCreditAllocation(invoiceId, creditNoteId, amount, appliedOn);
    }
}
