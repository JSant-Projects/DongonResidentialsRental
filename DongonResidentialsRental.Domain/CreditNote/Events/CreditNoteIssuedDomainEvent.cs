using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.CreditNote.Events;

public sealed record CreditNoteIssuedDomainEvent(
    CreditNoteId CreditNoteId, 
    LeaseId LeaseId,
    Money Amount,
    DateOnly IssuedOn) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
