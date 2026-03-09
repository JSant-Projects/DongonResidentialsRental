using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Payment.Events;

public sealed record PaymentAppliedToInvoiceDomainEvent(
    PaymentId PaymentId, 
    InvoiceId InvoiceId, 
    Money Amount, 
    DateOnly AppliedOn): IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
