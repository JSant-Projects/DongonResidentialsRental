using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.MarkUnitAvailable;

public sealed class MarUnitAvailableCommandHandler : ICommandHandler<MarUnitAvailableCommand, Unit>
{

    private readonly IUnitRepository _unitRepository;
    public MarUnitAvailableCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }
    public async Task<Unit> Handle(MarUnitAvailableCommand command, CancellationToken cancellationToken)
    {

        var unit = await _unitRepository.GetByIdAsync(
                                    command.UnitId,
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), command.UnitId);
        }

        // Update status to Available
        unit.MarkAvailable();

        return Unit.Value;
    }
}
