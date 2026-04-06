using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoiceForPayment;

public sealed class GetOutstandingInvoicesForPaymentQueryHandler
    : IQueryHandler<GetOutstandingInvoicesForPaymentQuery, IReadOnlyList<OutstandingInvoicesForPaymentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOutstandingInvoicesForPaymentQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OutstandingInvoicesForPaymentResponse>> Handle(
        GetOutstandingInvoicesForPaymentQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.LeaseId == request.LeaseId)
            .Select(i => new InvoiceOutstandingItem
            {
                InvoiceId = i.InvoiceId.Id,
                InvoiceNumber = i.InvoiceNumber,
                DueDate = i.DueDate,
                BillingPeriodFrom = i.BillingPeriod.From,
                Currency = i.Currency,
                TotalAmount = i.Lines.Sum(l => (decimal?)(l.UnitPrice.Amount * l.Quantity)) ?? 0m,
                AmountPaid = i.Allocations.Sum(a => (decimal?)a.Amount.Amount) ?? 0m,
                AmountCredited = i.CreditAllocations.Sum(ca => (decimal?)ca.Amount.Amount) ?? 0m
            });

        var invoices = await query
            .Where(i => i.TotalAmount - i.AmountPaid - i.AmountCredited > 0m)
            .OrderByDescending(i => i.BillingPeriodFrom)
            .ThenByDescending(i => i.DueDate)
            .Select(i => new OutstandingInvoicesForPaymentResponse(
                i.InvoiceId,
                i.InvoiceNumber,
                i.DueDate,
                i.TotalAmount - i.AmountPaid - i.AmountCredited,
                i.Currency))
            .ToListAsync(cancellationToken);

        return invoices;
    }

    private sealed class InvoiceOutstandingItem
    {
        public Guid InvoiceId { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public DateOnly DueDate { get; init; }
        public DateOnly BillingPeriodFrom { get; init; }
        public string Currency { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public decimal AmountPaid { get; init; }
        public decimal AmountCredited { get; init; }
    }
}