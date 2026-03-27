using DongonResidentialsRental.Domain.Building;
using DomainUnit = DongonResidentialsRental.Domain.Unit.Unit;
using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.UnitTests.Unit;

public class UnitSpecifications
{
    // ---------- Helpers ----------
    private static BuildingId AnyBuildingId() => new BuildingId(Guid.NewGuid());
    private static BuildingId NullBuildingId() => null;


    // ---------- Create ----------
    [Theory]
    [InlineData("12B", 3)]
    [InlineData("5A", null)]
    public void Create_Should_Return_Unit_When_Unit_Is_Valid(string unitNumber, int? floor)
    {
        // Arrange
        var buildingId = AnyBuildingId();
        // Act
        var result = DomainUnit.Create(buildingId, unitNumber, floor);
        // Assert
        result.Should().BeOfType<DomainUnit>();
        result.Should().NotBeNull();
        result.BuildingId.Should().Be(buildingId);
        result.UnitNumber.Should().Be(unitNumber);
        result.Floor.Should().Be(floor);
        result.Status.Should().Be(UnitStatus.Active);
    }

    [Fact]
    public void Create_Should_Throw_ArgumentException_When_BuildingId_Is_Null()
    {
        // Arrange
        BuildingId buildingId = NullBuildingId();
        // Act
        Action act = () => DomainUnit.Create(buildingId, "12B", 3);
        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("BuildingId cannot be null*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_Should_Throw_ArgumentException_When_UnitNumber_Is_Null_Or_Empty(string unitNumber)
    {
        // Arrange
        var buildingId = new BuildingId(Guid.NewGuid());
        // Act
        Action act = () => DomainUnit.Create(buildingId, unitNumber, 3);
        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Unit number cannot be null or empty*");
    }

    // ---------- SetMaintenance ----------
    [Fact]
    public void SetMaintenance_Should_Set_Status_To_Maintenance()
    {
        // Arrange
        var buildingId = new BuildingId(Guid.NewGuid());
        var unit = DomainUnit.Create(buildingId, "12B", 3);
        // Act
        unit.PutUnderMaintenance();
        // Assert
        unit.Status.Should().Be(UnitStatus.Maintenance);
    }

    [Fact]
    public void SetMaintenance_Should_Throw_DomainException_When_Status_Is_Inactive()
    {
        // Arrange
        var buildingId = new BuildingId(Guid.NewGuid());
        var unit = DomainUnit.Create(buildingId, "12B", 3);
        // Act
        Action act = () => unit.Deactivate();
        act += () => unit.PutUnderMaintenance();
        // Assert
        act.Should().ThrowExactly<DomainException>().WithMessage("Cannot put an inactive unit under maintenance.");
    }

    // ---------- SetAvailable ----------
    [Fact]
    public void SetAvailable_Should_Set_Status_To_Available()
    {
        // Arrange
        var buildingId = new BuildingId(Guid.NewGuid());
        var unit = DomainUnit.Create(buildingId, "12B", 3);
        unit.PutUnderMaintenance();
        // Act
        unit.Activate();
        // Assert
        unit.Status.Should().Be(UnitStatus.Active);
    }

    // ---------- SetInactive ----------
    [Fact]
    public void SetInactive_Should_Set_Status_To_Inactive()
    {
        // Arrange
        var buildingId = new BuildingId(Guid.NewGuid());
        var unit = DomainUnit.Create(buildingId, "12B", 3);
        // Act
        unit.Deactivate();
        // Assert
        unit.Status.Should().Be(UnitStatus.Inactive);
    }
}
