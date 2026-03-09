using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.CreditNote.Events;

public sealed record CreditNoteAppliedToInvoiceDomainEvent(
    CreditNoteId CreditNoteId, 
    InvoiceId InvoiceId, 
    Money Amount, 
    DateOnly AppliedOn): IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
