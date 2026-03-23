using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnits;

public sealed class GetUnitsQueryHandler : IQueryHandler<GetUnitsQuery, PagedResult<UnitResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetUnitsQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<UnitResponse>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = _dbContext.Units
            .AsNoTracking()
            .ApplyStatusFilter(request.Status)
            .ApplyUnitNumberSearch(request.UnitNumber)
            .ApplyBuildingFilter(request.BuildingId)
            .ApplyFloorFilter(request.Floor);

        var totalCount = await units.CountAsync(cancellationToken);

        var items = await units
            .ApplyOrdering()
            .ApplyPaging(request.Page, request.PageSize)
            .Select(UnitMappings.ToResponse())
            .ToListAsync(cancellationToken);

        return new PagedResult<UnitResponse>(
            items, 
            request.Page, 
            request.PageSize, 
            totalCount);
    }
}