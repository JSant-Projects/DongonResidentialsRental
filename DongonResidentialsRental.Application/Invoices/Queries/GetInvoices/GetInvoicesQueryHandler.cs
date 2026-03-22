using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;

public sealed partial class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetInvoicesQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<InvoiceResponse>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var listQuery = InvoiceQueryBuilder
            .BuildListQuery(_dbContext)
            .ApplyLeaseFilter(request.LeaseId)
            .ApplyBillingPeriodFilter(request.Period)
            .ApplySearchFilter(request.SearchTerm);


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
                x.TenantName,
                x.BuildingName,
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
}

