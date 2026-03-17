using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Leases.Queries.GetLeaseById;

public sealed class GetLeaseByIdQueryHandler : IQueryHandler<GetLeaseByIdQuery, LeaseResponse>
{
    private readonly IApplicationDBContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public GetLeaseByIdQueryHandler(
        IApplicationDBContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext; 
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<LeaseResponse> Handle(GetLeaseByIdQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        var lease = await (
            from l in _dbContext.Leases.AsNoTracking()
            join t in _dbContext.Tenants.AsNoTracking() on l.Occupancy equals t.TenantId
            join u in _dbContext.Units.AsNoTracking() on l.UnitId equals u.UnitId
            join b in _dbContext.Buildings.AsNoTracking() on u.BuildingId equals b.BuildingId
            where l.LeaseId == request.LeaseId
            select new LeaseResponse(
                l.LeaseId,
                b.Name,
                u.UnitNumber,
                t.PersonalInfo.FirstName + " " + t.PersonalInfo.LastName,
                l.Term.StartDate,
                l.Term.EndDate,
                l.MonthlyRate.Amount,
                l.MonthlyRate.Currency,
                l.Term.StartDate <= today && l.Term.EndDate >= today && l.Status == LeaseStatus.Active

            ))
            .FirstOrDefaultAsync(cancellationToken);


        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        return lease;

    }
}