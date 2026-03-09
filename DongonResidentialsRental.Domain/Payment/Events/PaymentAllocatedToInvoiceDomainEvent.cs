using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Payment.Events;

public sealed record PaymentAllocatedToInvoiceDomainEvent(
    PaymentId PaymentId, 
    InvoiceId InvoiceId, 
    Money Amount, 
    DateOnly AllocatedOn): IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
