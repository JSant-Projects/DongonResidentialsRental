using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed class InvoiceAllocation
{
    public InvoiceAllocationId InvoiceAllocationId { get; }
    public InvoiceId InvoiceId { get; }
    public PaymentId PaymentId { get; } 
    public Money Amount { get; } 
    public DateOnly AppliedOn { get; }

    private InvoiceAllocation() { } // For EF Core

    private InvoiceAllocation(
        InvoiceAllocationId invoiceAllocationId,
        InvoiceId invoiceId, 
        PaymentId paymentId, 
        Money amount, 
        DateOnly appliedOn)
    {
        InvoiceAllocationId = invoiceAllocationId;
        InvoiceId = invoiceId;
        PaymentId = paymentId;
        Amount = amount;
        AppliedOn = appliedOn;
    }

    internal static InvoiceAllocation Create(
        InvoiceId invoiceId,
        PaymentId paymentId,
        Money amount,
        DateOnly appliedOn)
    {
        Ensure.NotNull(paymentId);
        Ensure.NotNull(amount);

        if (amount.Amount <= 0)
            throw new DomainException("Allocation amount must be greater than zero.");

        if (appliedOn == default)
            throw new DomainException("AppliedOn is required.");

        return new InvoiceAllocation(
            new InvoiceAllocationId(Guid.NewGuid()),
            invoiceId, 
            paymentId, 
            amount, 
            appliedOn);
    }
}
