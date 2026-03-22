using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoiceDetails;

public sealed class GetInvoiceDetailsQueryHandler : IQueryHandler<GetInvoiceDetailsQuery, InvoiceDetailsResponse>
{
    private readonly IApplicationDBContext _dbContext;
    public GetInvoiceDetailsQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<InvoiceDetailsResponse> Handle(GetInvoiceDetailsQuery request, CancellationToken cancellationToken)
    {
        var invoice = await
        (from i in _dbContext.Invoices.AsNoTracking()
         join l in _dbContext.Leases.AsNoTracking()
             on i.LeaseId equals l.LeaseId
         join t in _dbContext.Tenants.AsNoTracking()
             on l.Occupancy equals t.TenantId
         join u in _dbContext.Units.AsNoTracking()
             on l.UnitId equals u.UnitId
         where i.InvoiceId == request.InvoiceId
         select new InvoiceResponse(
             i.InvoiceId.Id,
             i.InvoiceNumber,
             l.LeaseId.Id,
             t.TenantId.Id,
             t.PersonalInfo.FirstName + " " + t.PersonalInfo.LastName,
             u.UnitId.Id,
             u.UnitNumber,
             i.BillingPeriod.From,
             i.BillingPeriod.To,
             i.DueDate,
             i.Status,
             i.Total.Amount,
             i.Balance.Amount,
             i.Total.Currency))
        .FirstOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        // Get invoice lines
        var invoiceLines = await GetInvoiceLinesAsync(request.InvoiceId, cancellationToken);

        // Get invoice allocations
        var allocations = await GetInvoicePaymentsAsync(request.InvoiceId, cancellationToken);

        // Get invoice credit allocations
        var credits = await GetInvoiceCreditsAsync(request.InvoiceId, cancellationToken);

        return new InvoiceDetailsResponse(
            invoice.InvoiceId,
            invoice.InvoiceNumber,
            invoice.LeaseId,
            invoice.TenantId,
            invoice.TenantName,
            invoice.UnitId,
            invoice.UnitNumber,
            invoice.From,
            invoice.To,
            invoice.DueDate,
            invoice.Status,
            invoice.TotalAmount,
            invoice.Balance,
            invoice.Currency,
            invoiceLines,
            allocations,
            credits);
    }

    

    private async Task<IReadOnlyList<InvoiceLineResponse>> GetInvoiceLinesAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    {
        return await _dbContext.InvoiceLines
            .AsNoTracking()
            .Where(l => l.InvoiceId == invoiceId)
            .Select(InvoiceMappings.ToInvoiceLineResponse())
            .ToListAsync(cancellationToken);
    }
    private async Task<IReadOnlyList<InvoicePaymentResponse>> GetInvoicePaymentsAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    {
        return await _dbContext.InvoicesAllocations
            .AsNoTracking()
            .Where(l => l.InvoiceId == invoiceId)
            .Select(InvoiceMappings.ToInvoicePaymentResponse())
            .ToListAsync(cancellationToken);
    }
    private async Task<IReadOnlyList<InvoiceCreditResponse>> GetInvoiceCreditsAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    {
        return await _dbContext.InvoicesCreditAllocations
            .AsNoTracking()
            .Where(l => l.InvoiceId == invoiceId)
            .Select(InvoiceMappings.ToInvoiceCreditResponse())
            .ToListAsync(cancellationToken);
    }
}