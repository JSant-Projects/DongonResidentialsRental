using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildingById;

public class GetBuildingByIdQueryHandler : IQueryHandler<GetBuildingByIdQuery, BuildingResponse>
{
    private readonly IApplicationDBContext _dbContext;
    public GetBuildingByIdQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<BuildingResponse> Handle(GetBuildingByIdQuery query, CancellationToken cancellationToken)
    {
        var building = await _dbContext
            .Buildings
            .AsNoTracking()
            .Where(b => b.BuildingId == query.BuildingId)
            .Select(b => new BuildingResponse(
                b.BuildingId.Id,
                b.Name,
                b.Address.Street,
                b.Address.City,
                b.Address.Province,
                b.Address.PostalCode))
            .FirstOrDefaultAsync(cancellationToken);

        if (building is null)
        {
            throw new NotFoundException(nameof(Building), query.BuildingId);
        }

        return building;
    }
}
