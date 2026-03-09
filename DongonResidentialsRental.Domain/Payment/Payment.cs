using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment.Events;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Payment;

public sealed class Payment: AggregateRoot
{
    public PaymentId PaymentId { get; }
    public TenantId TenantId { get; }
    public Money Amount { get; }
    public DateOnly ReceivedOn { get; }
    public string? Reference { get; }
    public PaymentMethod Method { get; }
    private readonly List<PaymentAllocation> _allocations = new();
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public Money AllocatedAmount =>
        _allocations.Aggregate(
            Money.Zero(Amount.Currency),
            (sum, a) => sum.Add(a.Amount));
    public Money RemainingAmount => Amount.Subtract(AllocatedAmount);
    public PaymentStatus Status { get; private set; }
    public DateOnly? ReversedOn { get; private set; }
    public string? ReversalReason { get; private set; }
    private Payment() { }
    private Payment(TenantId tenantId, Money amount, DateOnly receivedOn, PaymentMethod method, string? reference)
    {
        PaymentId = new PaymentId(Guid.NewGuid());
        TenantId = tenantId;
        Amount = amount;
        ReceivedOn = receivedOn;
        Method = method;
        Reference = reference;
        Status = PaymentStatus.Received;
    }

    public static Payment Create(
       TenantId tenantId,
       Money amount,
       DateOnly receivedOn,
       PaymentMethod method,
       string? reference = null)
    {
        Ensure.NotNull(tenantId);
        Ensure.NotNull(amount);

        if (amount.Amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        var payment = new Payment(tenantId, amount, receivedOn, method, reference);

        payment.AddDomainEvent(new PaymentReceivedDomainEvent(payment.PaymentId, tenantId, amount, receivedOn, method));

        return payment;
    }

    private void EnsureIsReceived()
    {
        if (Status is PaymentStatus.Received)
            return;

        throw new DomainException("Operation allowed only when payment is in Received state.");
    }   

    public void ApplyToInvoice(
      InvoiceId invoiceId,
      Money amount,
      DateOnly appliedOn)
    {
        Ensure.NotNull(invoiceId);
        Ensure.NotNull(amount);
        EnsureIsReceived();

        if (amount.Amount <= 0)
            throw new DomainException("Allocation amount must be greater than zero.");

        if (amount.Currency != Amount.Currency)
            throw new DomainException("Allocation currency must match payment currency.");

        if (amount.Amount > RemainingAmount.Amount)
            throw new DomainException("Allocation amount cannot exceed remaining payment amount.");

        var allocation = PaymentAllocation.Create(invoiceId, amount, appliedOn);

        _allocations.Add(allocation);

        AddDomainEvent(new PaymentAppliedToInvoiceDomainEvent(PaymentId, invoiceId, amount, appliedOn));
    }

    public void Reverse(DateOnly reversedOn, string reason)
    {
        if (Status is PaymentStatus.Reversed)
            throw new DomainException("Payment is already reversed.");

        Ensure.NotNullOrWhiteSpace(reason);

        Status = PaymentStatus.Reversed;
        ReversedOn = reversedOn;
        ReversalReason = reason;

        AddDomainEvent(new PaymentReversedDomainEvent(PaymentId, reversedOn, reason));
    }

}
