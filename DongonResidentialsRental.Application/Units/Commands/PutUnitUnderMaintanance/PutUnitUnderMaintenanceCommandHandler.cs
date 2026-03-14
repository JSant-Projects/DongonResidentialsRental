using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.PutUnitUnderMaintanance;

public sealed class PutUnitUnderMaintenanceCommandHandler : ICommandHandler<PutUnitUnderMaintenanceCommand, Unit>
{
    private readonly IUnitRepository _unitRepository;
    public PutUnitUnderMaintenanceCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }
    public async Task<Unit> Handle(PutUnitUnderMaintenanceCommand command, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(
                                    command.UnitId, 
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), command.UnitId);
        }

        // Update status to UnderMaintenance
        unit.PutUnderMaintenance();

        return Unit.Value;
    }
}
