using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoiceForPayment;

public sealed class GetOutstandingInvoicesForPaymentQueryHandler :
    IQueryHandler<GetOutstandingInvoicesForPaymentQuery, IReadOnlyList<OutstandingInvoicesForPaymentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public GetOutstandingInvoicesForPaymentQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<OutstandingInvoicesForPaymentResponse>> Handle(GetOutstandingInvoicesForPaymentQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => 
                i.LeaseId == request.LeaseId &&
                i.Balance.Amount > 0m)
            .OrderByDescending(i => i.BillingPeriod.From)
            .ThenByDescending(i => i.DueDate)
            .Select(i => 
                new OutstandingInvoicesForPaymentResponse(
                        i.InvoiceId.Id,
                        i.InvoiceNumber,
                        i.DueDate,
                        i.Balance.Amount,
                        i.Balance.Currency
                ))
            .ToListAsync(cancellationToken);

        return invoices;
    }
}