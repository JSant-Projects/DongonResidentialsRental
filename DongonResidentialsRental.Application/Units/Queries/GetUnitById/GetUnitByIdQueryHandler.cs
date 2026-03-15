using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using Unit = DongonResidentialsRental.Domain.Unit.Unit;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnitById;

public sealed class GetUnitByIdQueryHandler : IQueryHandler<GetUnitByIdQuery, UnitResponse>
{
    private readonly IApplicationDBContext _dbContenxt;
    public GetUnitByIdQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContenxt = dbContext;
    }
    public async Task<UnitResponse> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _dbContenxt.Units
            .AsNoTracking()
            .Where(u => u.UnitId == request.UnitId)
            .Select(UnitMappings.ToResponse())
            .FirstOrDefaultAsync(cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Unit), request.UnitId);
        }

        return unit;
    }
}