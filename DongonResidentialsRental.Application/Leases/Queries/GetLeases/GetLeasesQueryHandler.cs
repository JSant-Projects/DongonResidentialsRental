using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Building;
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
        var today = _dateTimeProvider.Today;

        var query = BuildLeaseListQuery();

        query = ApplyFilters(query, request, today);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.StartDate)
            .ThenBy(x => x.TenantFullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LeaseResponse(
                x.LeaseId.Id,
                x.BuildingName,
                x.UnitNumber,
                x.TenantFullName,
                x.StartDate,
                x.EndDate,
                x.RentAmount,
                x.Currency,
                x.StartDate <= today &&
                (x.EndDate == null || x.EndDate >= today) &&
                x.Status == Domain.Lease.LeaseStatus.Active))
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaseResponse>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }

    private IQueryable<LeaseListRow> BuildLeaseListQuery()
    {
        return
            from lease in _dbContext.Leases.AsNoTracking()
            join tenant in _dbContext.Tenants.AsNoTracking()
                on lease.Occupancy equals tenant.TenantId
            join unit in _dbContext.Units.AsNoTracking()
                on lease.UnitId equals unit.UnitId
            join building in _dbContext.Buildings.AsNoTracking()
                on unit.BuildingId equals building.BuildingId
            select new LeaseListRow
            {
                LeaseId = lease.LeaseId,
                TenantFullName = tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName,
                UnitNumber = unit.UnitNumber,
                BuildingName = building.Name,
                BuildingId = building.BuildingId,
                StartDate = lease.Term.StartDate,
                EndDate = lease.Term.EndDate,
                RentAmount = lease.MonthlyRate.Amount,
                Currency = lease.MonthlyRate.Currency,
                Status = lease.Status
            };
    }

    private static IQueryable<LeaseListRow> ApplyFilters(
        IQueryable<LeaseListRow> query,
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
            var buildingId = new BuildingId(request.BuildingId.Value);

            query = query.Where(x => x.BuildingId == buildingId);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                query = query.Where(x =>
                    x.StartDate <= today &&
                    (x.EndDate == null || x.EndDate >= today) &&
                    x.Status == Domain.Lease.LeaseStatus.Active);
            }
            else
            {
                query = query.Where(x =>
                    x.StartDate > today ||
                    (x.EndDate != null && x.EndDate < today) ||
                    x.Status != Domain.Lease.LeaseStatus.Active);
            }
        }

        return query;
    }

    private sealed class LeaseListRow
    {
        public LeaseId LeaseId { get; init; }
        public string TenantFullName { get; init; } = string.Empty;
        public string UnitNumber { get; init; } = string.Empty;
        public string BuildingName { get; init; } = string.Empty;
        public BuildingId BuildingId { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public decimal RentAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public Domain.Lease.LeaseStatus Status { get; init; }
    }
}