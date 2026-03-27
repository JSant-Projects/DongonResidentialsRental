using DongonResidentialsRental.Domain.Meter;
using DongonResidentialsRental.Domain.Unit;
using DomainMeter = DongonResidentialsRental.Domain.Meter.Meter;
using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.UnitTests.Meter;

public class MeterSpecifications
{
    [Theory]
    [InlineData(MeterType.Electricity)]
    [InlineData(MeterType.Water)]
    public void Create_Should_Return_Meter_When_Meter_Is_Valid(MeterType meterType)
    {
        // Arrange
        var unitId = new UnitId(Guid.NewGuid());
        // Act
        var result = DomainMeter.Create(unitId, meterType);
        // Assert
        result.Should().BeOfType<DomainMeter>();
        result.Should().NotBeNull();
        result.UnitId.Should().Be(unitId);
        result.Type.Should().Be(meterType);
        result.Status.Should().Be(MeterStatus.Active);
    }


    [Fact]
    public void Create_Should_Throw_ArgumentException_When_UnitId_Is_Null()
    {
        // Arrange
        UnitId unitId = null;
        // Act
        Action act = () => DomainMeter.Create(unitId, MeterType.Electricity);
        // Assert
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Unit ID cannot be null*");
    }

    [Theory]
    [InlineData(2026, 01, 04, 100)]
    [InlineData(2026, 02, 04, 200)]
    [InlineData(2026, 03, 04, 300)]
    public void AddReading_Should_Add_Reading_When_Meter_Is_Active(int year, int month, int day, decimal value)
    {
        // Arrange
        var unitId = new UnitId(Guid.NewGuid());
        var meter = DomainMeter.Create(unitId, MeterType.Water);
        var date = DateOnly.FromDateTime(new DateTime(year, month, day));
        // Act
        meter.AddReading(date, value);
        // Assert
        meter.Readings.Should().HaveCount(1);
        meter.Readings[0].Date.Should().Be(date);
        meter.Readings[0].Value.Should().Be(value);
    }

    [Fact]
    public void AddReading_Should_Throw_DomainException_When_Meter_Is_Inactive()
    {
        // Arrange
        var unitId = new UnitId(Guid.NewGuid());
        var meter = DomainMeter.Create(unitId, MeterType.Water);
        meter.Deactivate();
        var date = DateOnly.FromDateTime(new DateTime(2026, 01, 04));
        // Act
        Action act = () => meter.AddReading(date, 100);
        // Assert
        act.Should().ThrowExactly<DomainException>().WithMessage("Meter is not active.");
    }

    [Fact]
    public void AddReading_Should_Throw_DomainException_When_Reading_Date_Is_Not_Later_Than_Last_Reading()
    {
        // Arrange
        var unitId = new UnitId(Guid.NewGuid());
        var meter = DomainMeter.Create(unitId, MeterType.Water);
        var date1 = DateOnly.FromDateTime(new DateTime(2026, 01, 04));
        var date2 = DateOnly.FromDateTime(new DateTime(2026, 01, 03));
        meter.AddReading(date1, 100);
        // Act
        Action act = () => meter.AddReading(date2, 200);
        // Assert
        act.Should().ThrowExactly<DomainException>().WithMessage("Reading date must be later than last reading.");
    }

    [Fact]
    public void AddReading_Should_Throw_DomainException_When_Reading_Value_Is_Less_Than_Last_Reading()
    {
        // Arrange
        var unitId = new UnitId(Guid.NewGuid());
        var meter = DomainMeter.Create(unitId, MeterType.Water);
        var date1 = DateOnly.FromDateTime(new DateTime(2026, 01, 04));
        var date2 = DateOnly.FromDateTime(new DateTime(2026, 01, 05));
        meter.AddReading(date1, 100);
        // Act
        Action act = () => meter.AddReading(date2, 50);
        // Assert
        act.Should().ThrowExactly<DomainException>().WithMessage("Reading value cannot be less than last reading.");
    }
}