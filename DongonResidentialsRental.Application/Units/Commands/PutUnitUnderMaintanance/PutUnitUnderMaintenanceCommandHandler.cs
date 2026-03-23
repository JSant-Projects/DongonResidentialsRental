using DongonResidentialsRental.Application.Abstractions.Clock;
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
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILeaseRepository _leaseRepository;
    public PutUnitUnderMaintenanceCommandHandler(
        IUnitRepository unitRepository,
        IDateTimeProvider dateTimeProvider,
        ILeaseRepository leaseRepository)
    {
        _unitRepository = unitRepository;
        _dateTimeProvider = dateTimeProvider;
        _leaseRepository = leaseRepository;
    }
    public async Task<Unit> Handle(PutUnitUnderMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(
                                    request.UnitId, 
                                    cancellationToken);

        if (unit is null)
        {
            throw new NotFoundException(nameof(Domain.Unit), request.UnitId);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        bool hasActiveLease = await _leaseRepository.ExistsActiveLeaseForUnitAsync(
            unit.UnitId,
            today,
            cancellationToken);

        if (hasActiveLease)
        {
            throw new ConflictException($"{unit.UnitNumber} has an active lease. PutUnderMaintenance operation is not allowed");
        }

        // Update status to UnderMaintenance
        unit.PutUnderMaintenance();

        return Unit.Value;
    }
}
