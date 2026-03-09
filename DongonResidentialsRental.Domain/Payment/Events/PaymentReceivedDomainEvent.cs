using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Payment.Events
{
    public sealed record PaymentReceivedDomainEvent(
        PaymentId PaymentId, 
        TenantId TenantId,
        Money Amount,
        DateOnly ReceivedOn,
        PaymentMethod Method) : IDomainEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
