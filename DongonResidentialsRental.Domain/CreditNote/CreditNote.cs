using DongonResidentialsRental.Domain.CreditNote.Events;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.CreditNote;

public sealed class CreditNote: AggregateRoot
{
    public CreditNoteId CreditNoteId { get; }
    public LeaseId LeaseId { get; }
    public Money Amount { get; }
    private readonly List<CreditAllocation> _allocations = [];
    public IReadOnlyCollection<CreditAllocation> Allocations => _allocations.AsReadOnly();

    public Money AmountApplied => _allocations.Aggregate(Money.Zero(Amount.Currency), (sum, a) => sum.Add(a.Amount));
    public Money RemainingAmount => Amount.Subtract(AmountApplied);
    public DateOnly? IssuedOn { get; private set; }
    public CreditNoteStatus Status { get; private set; }

    private CreditNote() { }
    private CreditNote(
        CreditNoteId creditNoteId,
        LeaseId leaseId, 
        Money amount)
    {
        CreditNoteId = creditNoteId;
        LeaseId = leaseId;
        Amount = amount;
        Status = CreditNoteStatus.Draft;
    }

    public static CreditNote Create(LeaseId leaseId, Money amount)
    {
        Ensure.NotNull(leaseId);
        Ensure.NotNull(amount);

        if (amount.Amount <= 0)
            throw new DomainException("Credit note amount must be greater than zero.");

        var creditNote = new CreditNote(
            new CreditNoteId(Guid.NewGuid()),
            leaseId, 
            amount);

        //creditNote.AddDomainEvent(new CreditNoteCreatedDomainEvent(creditNote.CreditNoteId, creditNote.LeaseId, creditNote.Amount));

        return creditNote;
    }
    private void EnsureIsDraft()
    {
        if (Status is CreditNoteStatus.Draft)
            return;

        throw new DomainException("Operation allowed only when credit note is in Draft state.");

    }

    private void EnsureIsIssued()
    {
        if (Status is CreditNoteStatus.Issued)
            return;
        throw new DomainException("Operation allowed only when credit note is in Issued state.");
    }   

    public void Issue(DateOnly issuedOn)
    {
        EnsureIsDraft();

        Status = CreditNoteStatus.Issued;
        IssuedOn = issuedOn;

        AddDomainEvent(new CreditNoteIssuedDomainEvent(CreditNoteId, LeaseId, Amount, issuedOn));
    }

    public void Void()
    {
        EnsureIsIssued();
        if (_allocations.Any())
            throw new DomainException("Cannot void a credit note that has been applied to invoices.");

        Status = CreditNoteStatus.Voided;

        AddDomainEvent(new CreditNoteVoidedDomainEvent(CreditNoteId, LeaseId));
    }

    public void RemoveAllocation(InvoiceId invoiceId)
    {
        Ensure.NotNull(invoiceId);

        var allocationsToRemove = _allocations
            .Where(a => a.InvoiceId == invoiceId)
            .ToList();

        if (allocationsToRemove.Count == 0)
            throw new DomainException("No allocation exists for this payment.");

        foreach (var allocation in allocationsToRemove)
        {
            _allocations.Remove(allocation);
        }

        AddDomainEvent(new CreditNoteAllocationRemovedDomainEvent(CreditNoteId, invoiceId));
    }

    public void ApplyToInvoice(InvoiceId invoiceId, Money amount, DateOnly appliedOn)
    {
        Ensure.NotNull(invoiceId);
        Ensure.NotNull(amount);

        EnsureIsIssued();

        if (amount.Amount > RemainingAmount.Amount)
            throw new DomainException("Cannot allocate more than remaining credit.");

        var allocation = CreditAllocation.Create(CreditNoteId, invoiceId, amount, appliedOn);

        _allocations.Add(allocation);

        AddDomainEvent(new CreditNoteAppliedToInvoiceDomainEvent(CreditNoteId, invoiceId, amount, appliedOn));
    }
}
