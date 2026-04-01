using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Leases.Queries.GetLeases;

public sealed record GetLeasesQueryHandler : IQueryHandler<GetLeasesQuery, PagedResult<LeaseResponse>>
{

    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public GetLeasesQueryHandler(
        IApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<PagedResult<LeaseResponse>> Handle(GetLeasesQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = BuildLeaseListQuery(today);

        query = ApplyFilters(query, request, today);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.StartDate)
            .ThenBy(x => x.TenantFullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LeaseResponse(
                x.LeaseId,
                x.BuildingName,
                x.UnitNumber,
                x.TenantFullName,
                x.StartDate,
                x.EndDate,
                x.RentAmount,
                x.Currency,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaseResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private IQueryable<LeaseListItem> BuildLeaseListQuery(DateOnly today)
    {
        return _dbContext.Leases
            .AsNoTracking()
            .Join(
                _dbContext.Tenants.AsNoTracking(),
                lease => lease.Occupancy,
                tenant => tenant.TenantId,
                (lease, tenant) => new { lease, tenant })
            .Join(
                _dbContext.Units.AsNoTracking(),
                x => x.lease.UnitId,
                unit => unit.UnitId,
                (x, unit) => new { x.lease, x.tenant, unit })
            .Join(
                _dbContext.Buildings.AsNoTracking(),
                x => x.unit.BuildingId,
                building => building.BuildingId,
                (x, building) => new LeaseListItem(
                    x.lease.LeaseId.Id,
                    x.tenant.PersonalInfo.FirstName + " " + x.tenant.PersonalInfo.LastName,
                    x.unit.UnitNumber,
                    building.Name,
                    building.BuildingId.Id,
                    x.lease.Term.StartDate,
                    x.lease.Term.EndDate,
                    x.lease.MonthlyRate.Amount,
                    x.lease.MonthlyRate.Currency,
                    x.lease.Term.StartDate <= today && 
                        (x.lease.Term.EndDate == null || 
                        x.lease.Term.EndDate >= today) && 
                        x.lease.Status == Domain.Lease.LeaseStatus.Active
                ));
    }

    private static IQueryable<LeaseListItem> ApplyFilters(
        IQueryable<LeaseListItem> query,
        GetLeasesQuery request,
        DateOnly today)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            var pattern = $"%{searchTerm}%";

            query = query.Where(x =>
                EF.Functions.Like(x.TenantFullName, pattern) ||
                EF.Functions.Like(x.UnitNumber, pattern) ||
                EF.Functions.Like(x.BuildingName, pattern));
        }

        if (request.BuildingId.HasValue)
        {
            query = query.Where(x => x.BuildingId == request.BuildingId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        return query;
    }

    private sealed record LeaseListItem(
        Guid LeaseId,
        string TenantFullName,
        string UnitNumber,
        string BuildingName,
        Guid BuildingId,
        DateOnly StartDate,
        DateOnly? EndDate,
        decimal RentAmount,
        string Currency,
        bool IsActive);
}