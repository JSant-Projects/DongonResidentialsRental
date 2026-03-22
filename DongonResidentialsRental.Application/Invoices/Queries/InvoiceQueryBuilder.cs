using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public static class InvoiceQueryBuilder
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
         join building in dbContext.Buildings.AsNoTracking()
            on unit.BuildingId equals building.BuildingId
         select new InvoiceListItem(
             invoice.InvoiceId.Id,
             invoice.InvoiceNumber,
             lease.LeaseId.Id,
             tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName,
             building.Name,
             unit.UnitNumber,
             invoice.BillingPeriod.From,
             invoice.BillingPeriod.To,
             invoice.DueDate,
             invoice.Status,
             invoice.Total.Amount,
             invoice.Balance.Amount,
             invoice.Total.Currency);
    }

    public static IQueryable<InvoiceListItem> ApplyLeaseFilter(
      this IQueryable<InvoiceListItem> query,
      LeaseId? leaseId)
    {
        if (leaseId is null)
            return query;

        return query.Where(i => i.LeaseId == leaseId.Id);
    }

    public static IQueryable<InvoiceListItem> WithOutstandingBalance(
      this IQueryable<InvoiceListItem> query)
    {
        return query.Where(x => x.Balance > 0m);
    }

    public static IQueryable<InvoiceListItem> ApplyBillingPeriodFilter(
        this IQueryable<InvoiceListItem> query,
        DateRange? range)
    {
        if (range is null)
            return query;

        return query.Where(x =>
                x.From <= range.To &&
                x.To >= range.From);
    }

    public static IQueryable<InvoiceListItem> ApplySearchFilter(
        this IQueryable<InvoiceListItem> query,
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
