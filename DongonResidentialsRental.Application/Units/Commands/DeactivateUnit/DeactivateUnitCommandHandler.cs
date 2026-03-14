using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.DeactivateUnit;

public sealed class DeactivateUnitCommandHandler : ICommandHandler<DeactivateUnitCommand, Unit>
{
    private readonly IUnitRepository _unitRepository;
    public DeactivateUnitCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }
    public async Task<Unit> Handle(DeactivateUnitCommand command, CancellationToken cancellationToken)
    {


        var unit = await _unitRepository.GetByIdAsync(
                                    command.UnitId,
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), command.UnitId);
        }

        // Update status to Inactive
        unit.Deactivate();

        return Unit.Value;
    }
}
