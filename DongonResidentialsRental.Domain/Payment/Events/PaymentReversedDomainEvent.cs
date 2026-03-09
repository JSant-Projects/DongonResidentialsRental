using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Payment.Events;

public sealed record PaymentReversedDomainEvent(
    PaymentId PaymentId,
    DateOnly ReversedOn,
    string ReversalReason) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
