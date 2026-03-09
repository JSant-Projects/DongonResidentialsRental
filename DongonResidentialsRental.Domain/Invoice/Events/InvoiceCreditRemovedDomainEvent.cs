using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Invoice.Events;

public sealed record InvoiceCreditRemovedDomainEvent(
    InvoiceId InvoiceId, 
    CreditNoteId CreditNoteId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
