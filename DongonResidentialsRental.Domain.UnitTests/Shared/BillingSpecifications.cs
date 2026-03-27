using AwesomeAssertions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.UnitTests.Shared;

public class BillingSpecifications
{
    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    [InlineData(-1)]
    public void Create_Should_Throw_ArgumentException_When_DueDayOfMonth_Is_Invalid(int dueDayOfMonth)
    {
        // Act
        Action act = () => BillingSettings.Create(dueDayOfMonth, 0);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("Due day of month must be between 1 and 28*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_Should_Throw_ArgumentException_When_GracePeriodDays_Is_Negative(int gracePeriodDays)
    {
        // Act
        Action act = () => BillingSettings.Create(5, gracePeriodDays);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("Grace period days cannot be negative*");
    }

    [Fact]
    public void Create_Should_Set_Properties()
    {
        // Act
        var result = BillingSettings.Create(10, 5);

        // Assert
        result.DueDayOfMonth.Should().Be(10);
        result.GracePeriodDays.Should().Be(5);
    }

    [Fact]
    public void CalculateDueDate_Should_Return_Correct_Date_Based_On_BillingPeriod_From()
    {
        // Arrange
        var billingSettings = BillingSettings.Create(10, 0);
        var billingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

        // Act
        var result = billingSettings.CalculateDueDate(billingPeriod);

        // Assert
        result.Should().Be(new DateOnly(2026, 3, 10));
    }

    [Fact]
    public void CalculateDueDate_Should_Use_DueDayOfMonth()
    {
        // Arrange
        var billingSettings = BillingSettings.Create(15, 0);
        var billingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));

        // Act
        var result = billingSettings.CalculateDueDate(billingPeriod);

        // Assert
        result.Day.Should().Be(15);
    }

    [Fact]
    public void CalculateLateAfterDate_Should_Add_GracePeriodDays_To_DueDate()
    {
        // Arrange
        var billingSettings = BillingSettings.Create(10, 5);
        var billingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

        // Act
        var result = billingSettings.CalculateLateAfterDate(billingPeriod);

        // Assert
        result.Should().Be(new DateOnly(2026, 3, 15));
    }
}
