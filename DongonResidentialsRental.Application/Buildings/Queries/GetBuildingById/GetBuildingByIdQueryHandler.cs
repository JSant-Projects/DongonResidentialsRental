using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildingById;

public class GetBuildingByIdQueryHandler : IQueryHandler<GetBuildingByIdQuery, BuildingResponse>
{
    private readonly IApplicationDbContext _dbContext;
    public GetBuildingByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<BuildingResponse> Handle(GetBuildingByIdQuery request, CancellationToken cancellationToken)
    {
        var building = await _dbContext
            .Buildings
            .AsNoTracking()
            .Where(b => b.BuildingId == request.BuildingId)
            .Select(BuildingMappings.ToResponse())
            .FirstOrDefaultAsync(cancellationToken);

        if (building is null)
        {
            throw new NotFoundException(nameof(Building), request.BuildingId);
        }

        return building;
    }
}
