using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public static class InvoiceQueryBuilder
{
    public static IQueryable<InvoiceListRow> BuildListQuery(IApplicationDbContext dbContext)
    {
        return
         from invoice in dbContext.Invoices.AsNoTracking()
         join lease in dbContext.Leases.AsNoTracking()
            on invoice.LeaseId equals lease.LeaseId
         join tenant in dbContext.Tenants.AsNoTracking()
            on lease.Occupancy equals tenant.TenantId
         join unit in dbContext.Units.AsNoTracking()
            on lease.UnitId equals unit.UnitId
         join building in dbContext.Buildings.AsNoTracking()
            on unit.BuildingId equals building.BuildingId
         select new InvoiceListRow
         {
             InvoiceId = invoice.InvoiceId.Id,
             InvoiceNumber = invoice.InvoiceNumber,
             LeaseId = lease.LeaseId.Id,
             TenantName = tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName,
             BuildingName = building.Name,
             UnitNumber = unit.UnitNumber,
             From = invoice.BillingPeriod.From,
             To = invoice.BillingPeriod.To,
             DueDate = invoice.DueDate,
             Status = invoice.Status,
             TotalAmount = invoice.Lines.Sum(l => (decimal?)(l.UnitPrice.Amount * l.Quantity)) ?? 0m,
             AmountPaid = invoice.Allocations.Sum(l => (decimal?)(l.Amount.Amount)) ?? 0m,
             AmountCredited = invoice.CreditAllocations.Sum(ca => (decimal?)ca.Amount.Amount) ?? 0m,
             Currency = invoice.Total.Currency,
             GracePeriodDays = lease.BillingSettings.GracePeriodDays
         };
    }

    public static IQueryable<InvoiceListRow> ApplyLeaseFilter(
      this IQueryable<InvoiceListRow> query,
      LeaseId? leaseId)
    {
        if (leaseId is null)
            return query;

        return query.Where(i => i.LeaseId == leaseId.Id);
    }

    public static IQueryable<InvoiceListRow> WithOutstandingBalance(
      this IQueryable<InvoiceListRow> query)
    {
        return query.Where(i => i.TotalAmount - i.AmountPaid - i.AmountCredited > 0m);
    }

    public static IQueryable<InvoiceListRow> WhereOverdue(
      this IQueryable<InvoiceListRow> query, 
      DateOnly today)
    {
        return query.Where(x => 
                x.DueDate.AddDays(x.GracePeriodDays) < today);
    }

    public static IQueryable<InvoiceListRow> WhereDueSoon(
      this IQueryable<InvoiceListRow> query,
      DateOnly today,
      int days)
    {
        return query.Where(x => x.DueDate == today.AddDays(days));
    }

    public static IQueryable<InvoiceListRow> ApplyBillingPeriodFilter(
        this IQueryable<InvoiceListRow> query,
        DateRange? range)
    {
        if (range is null)
            return query;

        return query.Where(x =>
                x.From <= range.To &&
                x.To >= range.From);
    }

    public static IQueryable<InvoiceListRow> ApplySearchFilter(
        this IQueryable<InvoiceListRow> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        searchTerm = searchTerm.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(x =>
            EF.Functions.Like(x.InvoiceNumber, pattern) ||
            EF.Functions.Like(x.TenantName, pattern) ||
            EF.Functions.Like(x.UnitNumber, pattern));
    }


}
