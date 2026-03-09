using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Invoice.Events;

public sealed record InvoiceDraftCreatedDomainEvent(
    InvoiceId InvoiceId, 
    LeaseId LeaseId, 
    BillingPeriod BillingPeriod) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
