using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Payment;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentDetails;

public sealed class GetPaymentDetailsQueryHandler : IQueryHandler<GetPaymentDetailsQuery, PaymentDetailsResponse>
{
    private readonly IApplicationDBContext _dbContext;
    public GetPaymentDetailsQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PaymentDetailsResponse> Handle(GetPaymentDetailsQuery request, CancellationToken cancellationToken)
    {
        var payment = await (
            from p in _dbContext.Payments.AsNoTracking()
            join t in _dbContext.Tenants.AsNoTracking()
                on p.TenantId equals t.TenantId
            where p.PaymentId == request.PaymentId
            select new PaymentResponse(
                p.PaymentId.Id,
                t.PersonalInfo.FirstName + " " + t.PersonalInfo.LastName,
                p.Amount.Amount,
                p.Amount.Currency,
                p.ReceivedOn,
                p.Reference,
                p.Method))
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(nameof(Payment), request.PaymentId);
        }

        var allocations = await GetAllocationsAsync(request.PaymentId, cancellationToken);

        return new PaymentDetailsResponse(
            payment.PaymentId,
            payment.TenantName,
            payment.Amount,
            payment.Currency,
            payment.ReceivedOn, 
            payment.Reference,
            payment.Method,
            allocations);
    }

    private async Task<IReadOnlyList<PaymentAllocationResponse>> GetAllocationsAsync(
        PaymentId PaymentId, 
        CancellationToken cancellationToken)
    {
        return await (
            from p in _dbContext.PaymentsAllocations.AsNoTracking()
            join i in _dbContext.Invoices.AsNoTracking()
                on p.InvoiceId equals i.InvoiceId
            select new PaymentAllocationResponse(
                i.InvoiceNumber,
                p.Amount.Amount,
                p.Amount.Currency,
                p.AllocatedOn))
            .ToListAsync(cancellationToken);
    }
}