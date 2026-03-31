using DongonResidentialsRental.Application.Abstractions.Clock;
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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILeaseRepository _leaseRepository;
    public DeactivateUnitCommandHandler(
        IUnitRepository unitRepository,
        IDateTimeProvider dateTimeProvider,
        ILeaseRepository leaseRepository)
    {
        _unitRepository = unitRepository;
        _dateTimeProvider = dateTimeProvider;
        _leaseRepository = leaseRepository;
    }
    public async Task<Unit> Handle(DeactivateUnitCommand request, CancellationToken cancellationToken)
    {


        var unit = await _unitRepository.GetByIdAsync(
                                    request.UnitId,
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), request.UnitId);
        }

        var today = _dateTimeProvider.Today;

        bool hasActiveLease = await _leaseRepository.ExistsActiveLeaseForUnitAsync(
            unit.UnitId,
            today,
            cancellationToken);

        if (hasActiveLease)
        {
            throw new ConflictException($"{unit.UnitNumber} has an active lease. Deactivate operation is not allowed");
        }

        // Update status to Inactive
        unit.Deactivate();

        return Unit.Value;
    }
}
