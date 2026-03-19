using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;

public static class BillingPeriodCalculator
{
    public static IEnumerable<BillingPeriod> GetMissingCompletedPeriods(
        DateOnly leaseStart,
        DateOnly? leaseEnd,
        DateOnly? latestPeriodEnd,
        DateOnly today)
    {
        var nextStart = latestPeriodEnd ?? leaseStart;

        while (true)
        {
            var nextEnd = nextStart.AddMonths(1);

            if (nextEnd > today)
                yield break;

            if (leaseEnd is not null && nextEnd > leaseEnd.Value)
                yield break;

            yield return BillingPeriod.Create(nextStart, nextEnd);

            nextStart = nextEnd;
        }
    }
}
