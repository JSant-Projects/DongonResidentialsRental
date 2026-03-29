using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Building;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Buildings.Commands.CreateBuilding;

public sealed class CreateBuildingCommandHandlerTests
{
    private readonly IBuildingRepository _buildingRepository = Substitute.For<IBuildingRepository>();

    private readonly CreateBuildingCommandHandler _handler;

    public CreateBuildingCommandHandlerTests()
    {
        _handler = new CreateBuildingCommandHandler(_buildingRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_Building_Name_Already_Exists()
    {
        // Arrange
        var command = new CreateBuildingCommand(
            "Dongon Residences",
            "123 Main Street",
            "Camrose",
            "Alberta",
            "T4V1A1");

        _buildingRepository
            .ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"A building with the name {command.Name} already exists.");

        _buildingRepository.DidNotReceive()
            .Add(Arg.Any<Building>());
    }

    [Fact]
    public async Task Handle_Should_Create_Building_Add_To_Repository_And_Return_BuildingId_When_Request_Is_Valid()
    {
        // Arrange
        var command = new CreateBuildingCommand(
            "Dongon Residences",
            "123 Main Street",
            "Camrose",
            "Alberta",
            "T4V1A1");

        _buildingRepository
            .ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        Building? addedBuilding = null;

        _buildingRepository
            .When(x => x.Add(Arg.Any<Building>()))
            .Do(callInfo => addedBuilding = callInfo.Arg<Building>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedBuilding.Should().NotBeNull();
        addedBuilding!.BuildingId.Should().Be(result);
        addedBuilding.Name.Should().Be("Dongon Residences");
        addedBuilding.Address.Street.Should().Be("123 Main Street");
        addedBuilding.Address.City.Should().Be("Camrose");
        addedBuilding.Address.Province.Should().Be("Alberta");
        addedBuilding.Address.PostalCode.Should().Be("T4V1A1");

        _buildingRepository.Received(1)
            .Add(Arg.Any<Building>());
    }

    [Fact]
    public async Task Handle_Should_Create_Building_Using_Command_Values()
    {
        // Arrange
        var command = new CreateBuildingCommand(
            "Maple Towers",
            "456 Queen Street",
            "Edmonton",
            "Alberta",
            "T5J0A1");

        _buildingRepository
            .ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        Building? addedBuilding = null;

        _buildingRepository
            .When(x => x.Add(Arg.Any<Building>()))
            .Do(callInfo => addedBuilding = callInfo.Arg<Building>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedBuilding.Should().NotBeNull();
        addedBuilding!.Name.Should().Be("Maple Towers");
        addedBuilding.Address.Street.Should().Be("456 Queen Street");
        addedBuilding.Address.City.Should().Be("Edmonton");
        addedBuilding.Address.Province.Should().Be("Alberta");
        addedBuilding.Address.PostalCode.Should().Be("T5J0A1");
    }
}
