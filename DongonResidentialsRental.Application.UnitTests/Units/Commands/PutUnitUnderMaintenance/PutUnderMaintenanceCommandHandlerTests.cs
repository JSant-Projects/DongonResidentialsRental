using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Units.Commands.PutUnitUnderMaintanance;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using DomainUnit = DongonResidentialsRental.Domain.Unit.Unit;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Units.Commands.PutUnitUnderMaintenance;

public sealed class PutUnitUnderMaintenanceCommandHandlerTests
{
    private readonly IUnitRepository _unitRepository = Substitute.For<IUnitRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();

    private readonly PutUnitUnderMaintenanceCommandHandler _handler;

    public PutUnitUnderMaintenanceCommandHandlerTests()
    {
        _handler = new PutUnitUnderMaintenanceCommandHandler(
            _unitRepository,
            _dateTimeProvider,
            _leaseRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Unit_Does_Not_Exist()
    {
        // Arrange
        var unitId = NewUnitId();
        var command = new PutUnitUnderMaintenanceCommand(unitId);

        _unitRepository
            .GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns((DomainUnit?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{unitId}*");

        await _leaseRepository.DidNotReceive()
            .ExistsActiveLeaseForUnitAsync(
                Arg.Any<UnitId>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_Unit_Has_Active_Lease()
    {
        // Arrange
        var today = new DateTime(2026, 3, 29, 10, 0, 0);
        var todayDateOnly = DateOnly.FromDateTime(today);

        var unit = CreateAvailableUnit("101");
        var command = new PutUnitUnderMaintenanceCommand(unit.UnitId);

        _unitRepository
            .GetByIdAsync(unit.UnitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        _dateTimeProvider.Today.Returns(today);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                unit.UnitId,
                todayDateOnly,
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"{unit.UnitNumber} has an active lease. PutUnderMaintenance operation is not allowed");
    }

    [Fact]
    public async Task Handle_Should_Put_Unit_Under_Maintenance_When_There_Is_No_Active_Lease()
    {
        // Arrange
        var today = new DateTime(2026, 3, 29, 10, 0, 0);
        var todayDateOnly = DateOnly.FromDateTime(today);

        var unit = CreateAvailableUnit("101");
        var command = new PutUnitUnderMaintenanceCommand(unit.UnitId);

        _unitRepository
            .GetByIdAsync(unit.UnitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        _dateTimeProvider.Today.Returns(today);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                unit.UnitId,
                todayDateOnly,
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        unit.Status.Should().Be(UnitStatus.Maintenance);
    }

    private static BuildingId NewBuildingId() => new BuildingId(Guid.NewGuid());
    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());

    private static DomainUnit CreateAvailableUnit(string unitNumber)
    {
        var unit = DomainUnit.Create(
            NewBuildingId(),
            unitNumber,
            1);

        unit.Activate();

        return unit;
    }
}
