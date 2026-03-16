using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Units.Queries.GetUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetAvailableUnitsLookupByBuilding;

public sealed class GetAvailableUnitsLookupByBuildingQueryHandler : IQueryHandler<GetAvailableUnitsLookupByBuildingQuery, IReadOnlyList<UnitLookupResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;    
    public GetAvailableUnitsLookupByBuildingQueryHandler(
        IApplicationDBContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<IReadOnlyList<UnitLookupResponse>> Handle(GetAvailableUnitsLookupByBuildingQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);
        // Fetch only units that is available and doesn't have an active lease
        var lookup = await _dbContext.Units
            .AsNoTracking()
            .Where(u => 
                u.BuildingId == request.BuildingId &&
                u.Status == Domain.Unit.UnitStatus.Active && 
                !_dbContext.Leases.Any(
                        l => l.UnitId == u.UnitId && 
                        l.Term.StartDate <= today && 
                        l.Term.EndDate >= today))
            .Select(UnitMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}
