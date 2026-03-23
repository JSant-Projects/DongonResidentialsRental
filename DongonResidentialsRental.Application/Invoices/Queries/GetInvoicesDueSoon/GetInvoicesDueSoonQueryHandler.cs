using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoicesDueSoon;

public sealed class GetInvoicesDueSoonQueryHandler : IQueryHandler<GetInvoicesDueSoonQuery, PagedResult<InvoiceResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public GetInvoicesDueSoonQueryHandler(
        IApplicationDBContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<PagedResult<InvoiceResponse>> Handle(GetInvoicesDueSoonQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        var listQuery = InvoiceQueryBuilder
            .BuildListQuery(_dbContext)
            .WithOutstandingBalance()
            .WhereDueSoon(today, request.Days);

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
