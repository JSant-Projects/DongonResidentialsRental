using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoices;

public sealed class GetOutstandingInvoicesQueryHandler : IQueryHandler<GetOutstandingInvoicesQuery, PagedResult<InvoiceResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public GetOutstandingInvoicesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<InvoiceResponse>> Handle(GetOutstandingInvoicesQuery request, CancellationToken cancellationToken)
    {
        var listQuery = InvoiceQueryBuilder
            .BuildListQuery(_dbContext)
            .ApplyLeaseFilter(request.LeaseId)
            .WithOutstandingBalance();

        var totalCount = await listQuery.CountAsync(cancellationToken);

        var items = await listQuery
            .OrderByDescending(x => x.From)
            .ThenByDescending(x => x.DueDate)
            .ApplyPaging(request.Page, request.PageSize)
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
                x.TotalAmount - x.AmountPaid - x.AmountCredited,
                x.Currency))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }
}
