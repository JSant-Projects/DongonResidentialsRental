using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.CreditNote.Events
{
    public sealed record CreditNoteAllocationRemovedDomainEvent(
        CreditNoteId CreditNoteId, 
        InvoiceId InvoiceId) : IDomainEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
