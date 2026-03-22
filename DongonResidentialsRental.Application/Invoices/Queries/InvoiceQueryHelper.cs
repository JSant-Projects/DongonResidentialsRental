using DongonResidentialsRental.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public static class InvoiceQueryHelper
{
    public static IQueryable<InvoiceListItem> BuildListQuery(IApplicationDBContext dbContext)
    {
        return
         from invoice in dbContext.Invoices.AsNoTracking()
         join lease in dbContext.Leases.AsNoTracking()
             on invoice.LeaseId equals lease.LeaseId
         join tenant in dbContext.Tenants.AsNoTracking()
             on lease.Occupancy equals tenant.TenantId
         join unit in dbContext.Units.AsNoTracking()
             on lease.UnitId equals unit.UnitId
         select new InvoiceListItem(
             invoice.InvoiceId.Id,
             invoice.InvoiceNumber,
             lease.LeaseId.Id,
             tenant.TenantId.Id,
             tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName,
             unit.UnitId.Id,
             unit.UnitNumber,
             invoice.BillingPeriod.From,
             invoice.BillingPeriod.To,
             invoice.DueDate,
             invoice.Status,
             invoice.Total.Amount,
             invoice.Balance.Amount,
             invoice.Total.Currency);
    }
}
