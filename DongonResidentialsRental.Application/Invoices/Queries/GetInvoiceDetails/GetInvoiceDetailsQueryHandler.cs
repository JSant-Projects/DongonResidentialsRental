using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoiceDetails;

public sealed class GetInvoiceDetailsQueryHandler : IQueryHandler<GetInvoiceDetailsQuery, InvoiceDetailsResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvoiceRepository _invoiceRepository;
    public GetInvoiceDetailsQueryHandler(
        IApplicationDbContext dbContext,
        IInvoiceRepository invoiceRepository)
    {
        _dbContext = dbContext;
        _invoiceRepository = invoiceRepository;
    }
    public async Task<InvoiceDetailsResponse> Handle(GetInvoiceDetailsQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository
                                .GetWithDetailsByIAsync(
                                    request.InvoiceId, 
                                    cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        var relatedInfo = await 
            (from l in _dbContext.Leases.AsNoTracking()
                join t in _dbContext.Tenants.AsNoTracking()
                    on l.Occupancy equals t.TenantId
                join u in _dbContext.Units.AsNoTracking()
                    on l.UnitId equals u.UnitId
                join b in _dbContext.Buildings.AsNoTracking()
                    on u.BuildingId equals b.BuildingId
                where l.LeaseId == invoice.LeaseId
                select new
                {
                    TenantName = t.PersonalInfo.FirstName + " " + t.PersonalInfo.LastName,
                    BuildingName = b.Name,
                    UnitNumber = u.UnitNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

        return new InvoiceDetailsResponse(
            invoice.InvoiceId.Id,
            invoice.InvoiceNumber,
            invoice.LeaseId.Id,
            relatedInfo!.TenantName,
            relatedInfo!.BuildingName,
            relatedInfo!.UnitNumber,
            invoice.BillingPeriod.From,
            invoice.BillingPeriod.To,
            invoice.DueDate,
            invoice.Status,
            invoice.Total.Amount,
            invoice.Balance.Amount,
            invoice.Currency,
            MapInvoiceLines(invoice.Lines),
            MapInvoicePayments(invoice.Allocations),
            MapInvoiceCredits(invoice.CreditAllocations));
    }

    public IReadOnlyList<InvoiceLineResponse> MapInvoiceLines(IReadOnlyCollection<InvoiceLine> lines)
    {
        return [.. lines.Select(l => new InvoiceLineResponse(
            l.Description,
            l.Quantity,
            l.UnitPrice.Amount,
            l.UnitPrice.Currency,
            l.Type))];
    }

    public IReadOnlyList<InvoicePaymentResponse> MapInvoicePayments(IReadOnlyCollection<InvoiceAllocation> allocations)
    {
        return [.. allocations.Select(a => new InvoicePaymentResponse(
            a.Amount.Currency,
            a.Amount.Amount,
            a.AppliedOn))];
    }

    public IReadOnlyList<InvoiceCreditResponse> MapInvoiceCredits(IReadOnlyCollection<InvoiceCreditAllocation> credits)
    {
        return [.. credits.Select(c => new InvoiceCreditResponse(
            c.Amount.Currency,
            c.Amount.Amount,
            c.AppliedOn))];
    }



    //private async Task<IReadOnlyList<InvoiceLineResponse>> GetInvoiceLinesAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    //{
    //    return await _dbContext.InvoiceLines
    //        .AsNoTracking()
    //        .Where(l => l.InvoiceId == invoiceId)
    //        .Select(InvoiceMappings.ToInvoiceLineResponse())
    //        .ToListAsync(cancellationToken);
    //}
    //private async Task<IReadOnlyList<InvoicePaymentResponse>> GetInvoicePaymentsAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    //{
    //    return await _dbContext.InvoicesAllocations
    //        .AsNoTracking()
    //        .Where(l => l.InvoiceId == invoiceId)
    //        .Select(InvoiceMappings.ToInvoicePaymentResponse())
    //        .ToListAsync(cancellationToken);
    //}
    //private async Task<IReadOnlyList<InvoiceCreditResponse>> GetInvoiceCreditsAsync(InvoiceId invoiceId, CancellationToken cancellationToken)
    //{
    //    return await _dbContext.InvoicesCreditAllocations
    //        .AsNoTracking()
    //        .Where(l => l.InvoiceId == invoiceId)
    //        .Select(InvoiceMappings.ToInvoiceCreditResponse())
    //        .ToListAsync(cancellationToken);
    //}
}