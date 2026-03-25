using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentsByTenant;

public sealed class GetPaymentsByTenantQueryHandler : IQueryHandler<GetPaymentsByTenantQuery, PagedResult<PaymentResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetPaymentsByTenantQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<PaymentResponse>> Handle(GetPaymentsByTenantQuery request, CancellationToken cancellationToken)
    {
        var payment = (
            from p in _dbContext.Payments.AsNoTracking()
            join t in _dbContext.Tenants.AsNoTracking()
                on p.TenantId equals t.TenantId
            where p.TenantId == request.TenantId
            select new PaymentResponse(
                p.PaymentId.Id,
                t.PersonalInfo.FirstName + " " + t.PersonalInfo.LastName,
                p.Amount.Amount,
                p.Amount.Currency,
                p.ReceivedOn,
                p.Reference,
                p.Method))
            .ApplyReferenceFilter(request.Reference)
            .ApplyReceivedOnFilter(request.ReceivedOn)
            .ApplyPaymentMethodFilter(request.Method);


        var totalCount = await payment.CountAsync(cancellationToken);

        var items = await payment
            .OrderByDescending(p => p.ReceivedOn)
            .ApplyPaging(request.Page, request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentResponse>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}
