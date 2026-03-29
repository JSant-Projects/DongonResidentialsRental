using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.ChangeMonthlyRate;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.ChangeMonthlyRate;

public sealed class ChangeMonthlyRateCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ChangeMonthlyRateCommandHandler _handler;

    public ChangeMonthlyRateCommandHandlerTests()
    {
        _handler = new ChangeMonthlyRateCommandHandler(
            _leaseRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();

        var command = new ChangeMonthlyRateCommand(
            leaseId,
            1500m,
            "CAD");

        _leaseRepository
            .GetByIdAsync(leaseId, Arg.Any<CancellationToken>())
            .Returns((Lease?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{leaseId}*");
    }

    [Fact]
    public async Task Handle_Should_Change_MonthlyRate_When_Lease_Exists()
    {
        // Arrange
        var lease = CreateDraftLease(
            monthlyRentAmount: 1200m,
            currency: "CAD");

        var command = new ChangeMonthlyRateCommand(
            lease.LeaseId,
            1500m,
            "CAD");

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateTime(2026, 3, 28, 9, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        lease.MonthlyRate.Currency.Should().Be("CAD");
        lease.MonthlyRate.Amount.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var lease = CreateDraftLease(
            monthlyRentAmount: 1200m,
            currency: "CAD");

        var command = new ChangeMonthlyRateCommand(
            lease.LeaseId,
            1700m, 
            "USD");

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateTime(2026, 3, 28));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());
    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());

    private static Lease CreateDraftLease(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        return Lease.Create(
            NewTenantId(),
            NewUnitId(),
            LeaseTerm.Create(startDate ?? new DateOnly(2026, 4, 1), endDate),
            Money.Create(currency, monthlyRentAmount),
            BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            UtilityResponsibility.Create(false, false));
    }
}
