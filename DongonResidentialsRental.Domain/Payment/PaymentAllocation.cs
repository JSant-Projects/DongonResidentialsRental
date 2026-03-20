using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.Payment;

public sealed class PaymentAllocation
{
    public PaymentAllocationId PaymentAllocationId { get; }
    public PaymentId PaymentId { get; }
    public InvoiceId InvoiceId { get; }
    public Money Amount { get; }
    public DateOnly AllocatedOn { get; }

    private PaymentAllocation() { }

    private PaymentAllocation(
        PaymentId paymentId,
        InvoiceId invoiceId, 
        Money amount, 
        DateOnly allocatedOn)
    {
        PaymentAllocationId = new PaymentAllocationId(Guid.NewGuid());
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        Amount = amount;
        AllocatedOn = allocatedOn;
    }

    internal static PaymentAllocation Create(
        PaymentId paymentId,
        InvoiceId invoiceId,
        Money amount,
        DateOnly allocatedOn)
    {
        Ensure.NotNull(invoiceId);
        Ensure.NotNull(amount);
        Ensure.NonNegativeDecimal(amount.Amount, "Allocation amount must be greater than zero.");

        if (allocatedOn == default)
            throw new DomainException("AllocatedOn is required.");

        return new PaymentAllocation(paymentId, invoiceId, amount, allocatedOn);
    }
}
