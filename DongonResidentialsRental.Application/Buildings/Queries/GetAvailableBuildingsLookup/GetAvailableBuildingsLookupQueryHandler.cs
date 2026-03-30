using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetAvailableBuildingsLookup;

public sealed class GetAvailableBuildingsLookupQueryHandler : IQueryHandler<GetAvailableBuildingsLookupQuery, IReadOnlyList<BuildingLookupResponse>>
{
    public readonly IApplicationDbContext _dbContext;
    public GetAvailableBuildingsLookupQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<BuildingLookupResponse>> Handle(GetAvailableBuildingsLookupQuery request, CancellationToken cancellationToken)
    {
        var lookup = await _dbContext.Buildings
            .AsNoTracking()
            .ApplyStatusFilter(BuildingStatus.Active)
            .ApplyOrdering()
            .Select(BuildingMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}
