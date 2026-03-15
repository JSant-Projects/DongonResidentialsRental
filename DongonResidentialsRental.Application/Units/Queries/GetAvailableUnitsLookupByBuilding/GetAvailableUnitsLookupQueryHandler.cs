using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Units.Queries.GetUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetAvailableUnitsLookupByBuilding;

public sealed class GetAvailableUnitsLookupQueryHandler : IQueryHandler<GetAvailableUnitsLookupByBuildingQuery, IReadOnlyList<UnitLookupResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetAvailableUnitsLookupQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<UnitLookupResponse>> Handle(GetAvailableUnitsLookupByBuildingQuery request, CancellationToken cancellationToken)
    {
        var lookup = await _dbContext.Units
            .AsNoTracking()
            .Where(u => 
                u.BuildingId == request.BuildingId 
                && u.Status == Domain.Unit.UnitStatus.Available)
            .Select(UnitMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}
