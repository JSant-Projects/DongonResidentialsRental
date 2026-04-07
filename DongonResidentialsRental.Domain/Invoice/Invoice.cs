using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice.Events;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class Invoice: AggregateRoot
{
    public InvoiceId InvoiceId { get; }
    public string InvoiceNumber { get; }
    public LeaseId LeaseId { get; }
    public BillingPeriod BillingPeriod { get; }
    private readonly List<InvoiceLine> _lines = new();
    private readonly List<InvoiceAllocation> _allocations = new();
    private readonly List<InvoiceCreditAllocation> _creditAllocations = new();
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<InvoiceAllocation> Allocations => _allocations.AsReadOnly();
    public IReadOnlyCollection<InvoiceCreditAllocation> CreditAllocations => _creditAllocations.AsReadOnly();
    public DateOnly DueDate { get; }
    public DateOnly? IssuedOn { get; private set; }
    public string Currency { get; }
    public Money Total =>
    _lines.Select(l => l.LineTotal)
          .DefaultIfEmpty(Money.Zero(Currency))
          .Aggregate((a, b) => a.Add(b));
    public Money AmountPaid =>
        _allocations.Select(a => a.Amount)
                    .DefaultIfEmpty(Money.Zero(Currency))
                    .Aggregate((a, b) => a.Add(b));
    public Money AmountCredited => _creditAllocations.Select(c => c.Amount)
                    .DefaultIfEmpty(Money.Zero(Currency))
                    .Aggregate((a, b) => a.Add(b));

    public Money Balance => 
        Total
            .Subtract(AmountPaid)
            .Subtract(AmountCredited);
    public InvoiceStatus Status { get; private set;  }

    private Invoice() { }
    private Invoice(
        InvoiceId invoiceId,
        LeaseId leaseId, 
        BillingPeriod billingPeriod, 
        DateOnly dueDate, 
        string currency, 
        string invoiceNumber)
    {
        InvoiceId = invoiceId;
        LeaseId = leaseId;
        BillingPeriod = billingPeriod;
        DueDate = dueDate;
        Currency = currency;
        Status = InvoiceStatus.Draft;
        InvoiceNumber = invoiceNumber;
    }

    private void EnsureIsDraft() 
    {
        if (Status is InvoiceStatus.Draft)
            return;

        throw new OperationNotAllowedException("Operation allowed only when invoice is in Draft state.");

    }

    private void EnsureCanAcceptPayment()
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new OperationNotAllowedException("Operation is not allowed when invoice is in Draft or Cancelled state.");

        if (Balance.Amount == 0)
            throw new OperationNotAllowedException("Invoice is already fully paid.");
    }

    private void EnsureCanApplyCredit()
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new OperationNotAllowedException("Operation is not allowed when invoice is in Draft or Cancelled state.");
        if (Balance.Amount == 0)
            throw new OperationNotAllowedException("Invoice is already fully paid.");
    }


    public void Issue(DateOnly issuedOn)
    {
        EnsureIsDraft();
        if (_lines.Count == 0)
            throw new OperationNotAllowedException("Cannot issue an invoice with no lines.");

        Status = InvoiceStatus.Issued;
        IssuedOn = issuedOn;

        AddDomainEvent(new InvoiceIssuedDomainEvent(InvoiceId, LeaseId, issuedOn));
    }

    public static Invoice Create(
        string invoiceNumber, 
        LeaseId leaseId, 
        BillingPeriod billingPeriod, 
        DateOnly dueDate, 
        string currency)
    {
        Ensure.NotNullOrWhiteSpace(invoiceNumber);
        Ensure.NotNull(leaseId);
        Ensure.NotNull(billingPeriod);
        Ensure.NotNullOrWhiteSpace(currency);
        currency = currency.Trim().ToUpperInvariant();
        Ensure.CharactersExactLength(currency, 3, "Currency must be a 3-letter ISO code.");
        Ensure.NotNull(dueDate);
        if (dueDate == default)
            throw new DomainException("Due date is required.");

        var invoice = new Invoice(
            new InvoiceId(Guid.NewGuid()),
            leaseId, 
            billingPeriod, 
            dueDate, 
            currency, 
            invoiceNumber);

        // Add domain event for invoice creation
        invoice.AddDomainEvent(new InvoiceDraftCreatedDomainEvent(invoice.InvoiceId, leaseId, billingPeriod));
        return invoice;
    }

    public void AddLine(string description, int quantity, Money unitPrice, InvoiceLineType type)
    {
        EnsureIsDraft();

        var existingLine = _lines.FirstOrDefault(l =>
            l.Type == type &&
            l.UnitPrice == unitPrice &&
            l.Description == description);

        if (existingLine is not null)
        {
            existingLine.IncreaseQuantity(quantity);
            return;
        }

        var line = InvoiceLine.Create(InvoiceId, description, quantity, unitPrice, type);

        if (unitPrice.Currency != Currency)
            throw new DomainException("Line currency must match invoice currency.");

        _lines.Add(line);
    }

    public void ApplyPayment(PaymentId paymentId, Money amount, DateOnly appliedOn)
    {
        EnsureCanAcceptPayment();

        if (amount.Currency != Currency)
            throw new DomainException("Payment currency must match invoice currency.");

        if (amount.Amount > Balance.Amount)
            throw new DomainException("Payment exceeds remaining balance.");

        if (_allocations.Any(a => a.PaymentId == paymentId))
            throw new OperationNotAllowedException("Payment has already been applied to this invoice.");

        var allocation = InvoiceAllocation.Create(InvoiceId, paymentId, amount, appliedOn);

        _allocations.Add(allocation);

        AddDomainEvent(new InvoicePaymentAppliedDomainEvent(InvoiceId, paymentId, amount, appliedOn));
    }

    public void ApplyCredit(CreditNoteId creditNoteId, Money amount, DateOnly appliedOn)
    {
        EnsureCanApplyCredit();

        if (amount.Currency != Currency)
            throw new DomainException("Credit currency must match invoice currency.");

        if (amount.Amount > Balance.Amount)
            throw new DomainException("Credit amount cannot exceed invoice balance.");

        _creditAllocations.Add(
            InvoiceCreditAllocation.Create(InvoiceId, creditNoteId, amount, appliedOn));

        AddDomainEvent(new InvoiceCreditAppliedDomainEvent(InvoiceId, creditNoteId, amount, appliedOn));
    }

    public void RemoveAllocation(PaymentId paymentId)
    {
        Ensure.NotNull(paymentId);

        var allocationsToRemove = _allocations
            .Where(a => a.PaymentId == paymentId)
            .ToList();

        if (allocationsToRemove.Count == 0)
            throw new OperationNotAllowedException("No allocation exists for this payment.");

        foreach (var allocation in allocationsToRemove)
        {
            _allocations.Remove(allocation);
        }

        AddDomainEvent(new InvoicePaymentRemovedDomainEvent(InvoiceId, paymentId));
    }

    public void RemoveCreditAllocation(CreditNoteId creditNoteId)
    {
        Ensure.NotNull(creditNoteId);

        var allocationsToRemove = _creditAllocations
            .Where(c => c.CreditNoteId == creditNoteId)
            .ToList();
        if (allocationsToRemove.Count == 0)
            throw new OperationNotAllowedException("No allocation exists for this credit note.");
        foreach (var allocation in allocationsToRemove)
        {
            _creditAllocations.Remove(allocation);

        }
        
        AddDomainEvent(new InvoiceCreditRemovedDomainEvent(InvoiceId, creditNoteId));
    }

    public void Cancel()
    {
        if (AmountPaid.Amount > 0)
            throw new OperationNotAllowedException("Cannot cancel an invoice that has been paid.");

        Status = InvoiceStatus.Cancelled;

        AddDomainEvent(new InvoiceCancelledDomainEvent(InvoiceId, LeaseId));
    }

    public bool HasLineOfType(InvoiceLineType type)
    {
        return _lines.Any(l => l.Type == type);
    }
}
