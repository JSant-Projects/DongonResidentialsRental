using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.CreditNote;

public sealed class CreditAllocation
{
    public CreditAllocationId CreditAllocationId { get; }
    public CreditNoteId CreditNoteId { get; }
    public InvoiceId InvoiceId { get; }
    public Money Amount { get; }
    public DateOnly AppliedOn { get; }

    private CreditAllocation() { } // For EF Core

    private CreditAllocation(CreditNoteId creditNoteId, InvoiceId invoiceId, Money amount, DateOnly appliedOn)
    {
        CreditAllocationId = new CreditAllocationId(Guid.NewGuid());
        CreditNoteId = creditNoteId;
        InvoiceId = invoiceId;
        Amount = amount;
        AppliedOn = appliedOn;
    }

    internal static CreditAllocation Create(
        CreditNoteId creditNoteId,
        InvoiceId invoiceId,
        Money amount,
        DateOnly appliedOn)
    {
        Ensure.NotNull(invoiceId);
        Ensure.NotNull(amount);

        if (amount.Amount <= 0)
            throw new DomainException("Allocation amount must be greater than zero.");

        if (appliedOn == default)
            throw new DomainException("AppliedOn is required.");

        return new CreditAllocation(creditNoteId, invoiceId, amount, appliedOn);
    }
}
