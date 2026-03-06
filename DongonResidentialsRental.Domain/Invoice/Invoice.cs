using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class Invoice
{
    public InvoiceId InvoiceId { get; }
    public LeaseId LeaseId { get; }
    public BillingPeriod BillingPeriod { get; }
    private readonly List<InvoiceLine> _lines = new();
    private readonly List<InvoiceAllocation> _allocations = new();
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();
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

    public Money Balance => Total.Subtract(AmountPaid);
    private InvoiceStatus _status;
    public InvoiceStatus Status =>
    _status switch
    {
        InvoiceStatus.Draft => InvoiceStatus.Draft,
        InvoiceStatus.Cancelled => InvoiceStatus.Cancelled,
        _ => (Balance.Amount, AmountPaid.Amount) switch
        {
            (0, _) => InvoiceStatus.Paid,
            ( > 0, > 0) => InvoiceStatus.PartiallyPaid,
            _ => InvoiceStatus.Issued
        }
    };

    private Invoice() { }
    private Invoice(LeaseId leaseId, BillingPeriod billingPeriod, string currency)
    {
        InvoiceId = new InvoiceId(Guid.NewGuid());
        LeaseId = leaseId;
        BillingPeriod = billingPeriod;
        Currency = currency;
        _status = InvoiceStatus.Draft;
    }

    private void EnsureIsDraft() 
    {
        if (Status is InvoiceStatus.Draft)
            return;

        throw new DomainException("Operation allowed only when invoice is in Draft state.");

    }

    private void EnsureIsIssued()
    {
        if (Status is InvoiceStatus.Issued)
            return;
        
        throw new DomainException("Operation allowed only when invoice is in Issued state.");
    }

    private void EnsureCanAcceptPayment()
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new DomainException("Operation is not allowed when invoice is in Draft or Cancelled state.");

        if (Balance.Amount == 0)
            throw new DomainException("Invoice is already fully paid.");
    }

    public void Issue(DateOnly issuedOn)
    {
        EnsureIsDraft();
        if (_lines.Count == 0)
            throw new DomainException("Cannot issue an invoice with no lines.");

        _status = InvoiceStatus.Issued;
        IssuedOn = issuedOn;
    }

    public static Invoice Create(LeaseId leaseId, BillingPeriod billingPeriod, string currency)
    {
        Ensure.NotNull(leaseId);
        Ensure.NotNull(billingPeriod);
        Ensure.NotNullOrWhiteSpace(currency);
        currency = currency.Trim().ToUpperInvariant();
        Ensure.CharactersExactLength(currency, 3, "Currency must be a 3-letter ISO code.");


        return new Invoice(leaseId, billingPeriod, currency);
    }

    public void AddLine(string description, int quantity, Money unitPrice, InvoiceLineType type)
    {
        EnsureIsDraft();

        var line = InvoiceLine.Create(description, quantity, unitPrice, type);

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
            throw new DomainException("Payment has already been applied to this invoice.");

        var allocation = InvoiceAllocation.Create(paymentId, amount, appliedOn);

        _allocations.Add(allocation);
    }

    public void RemoveAllocation(PaymentId paymentId)
    {
        Ensure.NotNull(paymentId);

        var allocationsToRemove = _allocations
            .Where(a => a.PaymentId == paymentId)
            .ToList();

        if (allocationsToRemove.Count == 0)
            throw new DomainException("No allocation exists for this payment.");

        foreach (var allocation in allocationsToRemove)
        {
            _allocations.Remove(allocation);
        }
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.PartiallyPaid)
            throw new DomainException("Cannot cancel an invoice that has been paid.");

        _status = InvoiceStatus.Cancelled;
    }

}
