using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Units.Commands.ActivateUnit;
using DongonResidentialsRental.Domain.Building;
using DomainUnit = DongonResidentialsRental.Domain.Unit.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using DongonResidentialsRental.Domain.Unit;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Units.Commands.ActivateUnit;

public sealed class ActivateUnitCommandHandlerTests
{
    private readonly IUnitRepository _unitRepository = Substitute.For<IUnitRepository>();

    private readonly ActivateUnitCommandHandler _handler;

    public ActivateUnitCommandHandlerTests()
    {
        _handler = new ActivateUnitCommandHandler(_unitRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Unit_Does_Not_Exist()
    {
        // Arrange
        var unitId = NewUnitId();
        var command = new ActivateUnitCommand(unitId);

        _unitRepository
            .GetByIdAsync(unitId, Arg.Any<CancellationToken>())
            .Returns((DomainUnit?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{unitId}*");
    }

    [Fact]
    public async Task Handle_Should_Activate_Unit_When_Unit_Exists()
    {
        // Arrange
        var unit = CreateInactiveUnit();
        var command = new ActivateUnitCommand(unit.UnitId);

        _unitRepository
            .GetByIdAsync(unit.UnitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        unit.Status.Should().Be(UnitStatus.Active);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var unit = CreateInactiveUnit();
        var command = new ActivateUnitCommand(unit.UnitId);

        _unitRepository
            .GetByIdAsync(unit.UnitId, Arg.Any<CancellationToken>())
            .Returns(unit);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());
    private static BuildingId NewBuildingId() => new BuildingId(Guid.NewGuid());

    private static DomainUnit CreateInactiveUnit()
    {
        var unit = DomainUnit.Create(
            buildingId: NewBuildingId(),
            unitNumber: "101",
            floor: 1);

        unit.Deactivate();

        return unit;
    }
}
