using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Invoice.Events;

public sealed record InvoiceIssuedDomainEvent(
    InvoiceId InvoiceId, 
    LeaseId LeaseId, 
    DateOnly IssuedDate) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
