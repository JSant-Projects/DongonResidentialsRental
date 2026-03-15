using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetAvailableBuildingsLookup;

public sealed class GetAvailableBuildingsLookupQueryHandler : IQueryHandler<GetAvailableBuildingsLookupQuery, IReadOnlyList<BuildingLookupResponse>>
{
    public readonly IApplicationDBContext _dbContext;
    public GetAvailableBuildingsLookupQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<BuildingLookupResponse>> Handle(GetAvailableBuildingsLookupQuery request, CancellationToken cancellationToken)
    {
        var lookup = await _dbContext.Buildings
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Where(b => b.Status == BuildingStatus.Active)
            .Select(BuildingMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}
