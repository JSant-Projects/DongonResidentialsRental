using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;

public sealed class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetInvoicesQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<InvoiceResponse>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var listQuery = BuildListQuery();

        listQuery = ApplyFilters(listQuery, request);

        var totalCount = await listQuery.CountAsync(cancellationToken);

        var items = await listQuery
            .OrderByDescending(x => x.From)
            .ThenByDescending(x => x.DueDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new InvoiceResponse(
                x.InvoiceId,
                x.LeaseId,
                x.TenantId,
                x.TenantName,
                x.UnitId,
                x.UnitNumber,
                x.From,
                x.To,
                x.DueDate,
                x.Status,
                x.TotalAmount,
                x.Balance,
                x.Currency))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private IQueryable<InvoiceListItem> BuildListQuery()
    {
        return
         from invoice in _dbContext.Invoices.AsNoTracking()
         join lease in _dbContext.Leases.AsNoTracking()
             on invoice.LeaseId equals lease.LeaseId
         join tenant in _dbContext.Tenants.AsNoTracking()
             on lease.Occupancy equals tenant.TenantId
         join unit in _dbContext.Units.AsNoTracking()
             on lease.UnitId equals unit.UnitId
         select new InvoiceListItem(
             invoice.InvoiceId.Id,
             lease.LeaseId.Id,
             tenant.TenantId.Id,
             tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName,
             unit.UnitId.Id,
             unit.UnitNumber,
             invoice.BillingPeriod.From,
             invoice.BillingPeriod.To,
             invoice.DueDate,
             invoice.Status.ToString(),
             invoice.Total.Amount,
             invoice.Balance.Amount,
             invoice.Total.Currency);
    }

    private static IQueryable<InvoiceListItem> ApplyFilters(
        IQueryable<InvoiceListItem> query,
        GetInvoicesQuery request)
    {
        if (request.LeaseId is not null)
        {
            query = query.Where(x => x.LeaseId == request.LeaseId.Id);
        }

        if (request.TenantId is not null)
        {
            query = query.Where(x => x.TenantId == request.TenantId.Id);
        }

        if (request.Period is not null)
        {
            query = query.Where(x =>
                x.From <= request.Period.To &&
                x.To >= request.Period.From);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            var pattern = $"%{searchTerm}%";

            query = query.Where(x =>
                EF.Functions.Like(x.TenantName, pattern) ||
                EF.Functions.Like(x.UnitNumber, pattern));
        }

        return query;
    }

    private sealed record InvoiceListItem(
        Guid InvoiceId,
        Guid LeaseId,
        Guid TenantId,
        string TenantName,
        Guid UnitId,
        string UnitNumber,
        DateOnly From,
        DateOnly To,
        DateOnly DueDate,
        string Status,
        decimal TotalAmount,
        decimal Balance,
        string Currency);
}

