using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;
using System.Security.Cryptography.X509Certificates;

namespace DongonResidentialsRental.Domain.Invoice;

public sealed record BillingPeriod(DateOnly From, DateOnly To)
{
    public static BillingPeriod Create(DateOnly from, DateOnly to)
    {
        if (to < from)
            throw new DomainException("Billing period end date cannot be before start date.");

        return new BillingPeriod(from, to);
    }

}
