using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.ActivateUnit;

public sealed class ActivateUnitCommandHandler : ICommandHandler<ActivateUnitCommand, Unit>
{

    private readonly IUnitRepository _unitRepository;
    public ActivateUnitCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }
    public async Task<Unit> Handle(ActivateUnitCommand request, CancellationToken cancellationToken)
    {

        var unit = await _unitRepository.GetByIdAsync(
                                    request.UnitId,
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), request.UnitId);
        }

        // Update status to Available
        unit.Activate();

        return Unit.Value;
    }
}
