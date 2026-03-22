using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoices;

public sealed class GetOutstandingInvoicesQueryHandler : IQueryHandler<GetOutstandingInvoicesQuery, PagedResult<InvoiceResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetOutstandingInvoicesQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<InvoiceResponse>> Handle(GetOutstandingInvoicesQuery request, CancellationToken cancellationToken)
    {
        var listQuery = InvoiceQueryHelper.BuildListQuery(_dbContext);

        listQuery = ApplyFilters(listQuery, request);

        listQuery = WithOutstandingBalance(listQuery);

        var totalCount = await listQuery.CountAsync(cancellationToken);

        var items = await listQuery
            .OrderByDescending(x => x.From)
            .ThenByDescending(x => x.DueDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new InvoiceResponse(
                x.InvoiceId,
                x.InvoiceNumber,
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

    private static IQueryable<InvoiceListItem> WithOutstandingBalance(
       IQueryable<InvoiceListItem> query)
    {
        return query.Where(x => x.Balance > 0m);
    }

    private static IQueryable<InvoiceListItem> ApplyFilters(
       IQueryable<InvoiceListItem> query,
       GetOutstandingInvoicesQuery request)
    {
        if (request.LeaseId is not null)
        {
            query = query.Where(x => x.LeaseId == request.LeaseId.Id);
        }

        if (request.TenantId is not null)
        {
            query = query.Where(x => x.TenantId == request.TenantId.Id);
        }

        return query;
    }
}
