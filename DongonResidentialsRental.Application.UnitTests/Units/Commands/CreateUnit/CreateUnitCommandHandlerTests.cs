using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Units.Commands.CreateUnit;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using DomainUnit = DongonResidentialsRental.Domain.Unit.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Units.Commands.CreateUnit;

public sealed class CreateUnitCommandHandlerTests
{
    private readonly IUnitRepository _unitRepository = Substitute.For<IUnitRepository>();
    private readonly IBuildingRepository _buildingRepository = Substitute.For<IBuildingRepository>();

    private readonly CreateUnitCommandHandler _handler;

    public CreateUnitCommandHandlerTests()
    {
        _handler = new CreateUnitCommandHandler(
            _unitRepository,
            _buildingRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Building_Does_Not_Exist()
    {
        // Arrange
        var buildingId = NewBuildingId();

        var command = new CreateUnitCommand(
            buildingId,
            "101",
            1);

        _buildingRepository
            .ExistsAsync(buildingId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{buildingId.Id}*");

        await _unitRepository.DidNotReceive()
            .ExistsUnitNumberInBuildingAsync(
                Arg.Any<BuildingId>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        _unitRepository.DidNotReceive()
            .Add(Arg.Any<DomainUnit>());
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_UnitNumber_Already_Exists_In_Building()
    {
        // Arrange
        var buildingId = NewBuildingId();

        var command = new CreateUnitCommand(
            buildingId,
            "101",
            1);

        _buildingRepository
            .ExistsAsync(buildingId, Arg.Any<CancellationToken>())
            .Returns(true);

        _unitRepository
            .ExistsUnitNumberInBuildingAsync(
                buildingId,
                "101",
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("A unit with the number 101 already exists in the building.");

        _unitRepository.DidNotReceive()
            .Add(Arg.Any<DomainUnit>());
    }

    [Fact]
    public async Task Handle_Should_Create_Unit_Add_To_Repository_And_Return_UnitId_When_Request_Is_Valid()
    {
        // Arrange
        var buildingId = NewBuildingId();

        var command = new CreateUnitCommand(
            buildingId,
            "305A",
            3);

        _buildingRepository
            .ExistsAsync(buildingId, Arg.Any<CancellationToken>())
            .Returns(true);

        _unitRepository
            .ExistsUnitNumberInBuildingAsync(
                buildingId,
                "305A",
                Arg.Any<CancellationToken>())
            .Returns(false);

        DomainUnit? addedUnit = null;

        _unitRepository
            .When(x => x.Add(Arg.Any<DomainUnit>()))
            .Do(callInfo => addedUnit = callInfo.Arg<DomainUnit>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedUnit.Should().NotBeNull();
        addedUnit!.UnitId.Should().Be(result);
        addedUnit.BuildingId.Should().Be(buildingId);
        addedUnit.UnitNumber.Should().Be("305A");
        addedUnit.Floor.Should().Be(3);

        _unitRepository.Received(1)
            .Add(Arg.Any<DomainUnit>());
    }

    private static BuildingId NewBuildingId() => new BuildingId(Guid.NewGuid());
}
