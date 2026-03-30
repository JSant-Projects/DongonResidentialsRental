using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;

public sealed class GetBuildingsQueryHandler : IQueryHandler<GetBuildingsQuery, PagedResult<BuildingResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public GetBuildingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<BuildingResponse>> Handle(GetBuildingsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Buildings
            .AsNoTracking()
            .ApplyStatusFilter(request.Status)
            .ApplySearch(request.SearchTerm);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyOrdering()
            .ApplyPaging(request.Page, request.PageSize)
            .Select(BuildingMappings.ToResponse())
            .ToListAsync(cancellationToken);

        return new PagedResult<BuildingResponse>(
            items, 
            request.Page, 
            request.PageSize, 
            totalCount);

    }
}
