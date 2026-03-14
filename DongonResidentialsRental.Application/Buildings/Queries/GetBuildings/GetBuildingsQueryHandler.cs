using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;

public sealed class GetBuildingsQueryHandler : IQueryHandler<GetBuildingsQuery, PagedResult<BuildingResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    public GetBuildingsQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<BuildingResponse>> Handle(GetBuildingsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Buildings
            .AsNoTracking()
            .AsQueryable();

        if (request.Status is not null)
        {
            query = query.Where(b => b.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();

            query = query.Where(b => b.Name.Contains(searchTerm) ||
                                     b.Address.Street.Contains(searchTerm) ||
                                     b.Address.City.Contains(searchTerm) ||
                                     b.Address.Province.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.Address.City)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BuildingResponse(
                b.BuildingId.Id,
                b.Name,
                b.Address.Street,
                b.Address.City,
                b.Address.Province,
                b.Address.PostalCode))
            .ToListAsync(cancellationToken);

        return new PagedResult<BuildingResponse>(
            items, 
            request.Page, 
            request.PageSize, 
            totalCount);

    }
}
