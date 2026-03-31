using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.ChangeBillingSettings;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.ChangeBillingSettings;

public sealed class ChangeBillingSettingsCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ChangeBillingSettingsCommandHandler _handler;

    public ChangeBillingSettingsCommandHandlerTests()
    {
        _handler = new ChangeBillingSettingsCommandHandler(
            _leaseRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();
        var command = new ChangeBillingSettingsCommand(
            leaseId,
            10,
            5);

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
    public async Task Handle_Should_Change_BillingSettings_When_Lease_Exists()
    {
        // Arrange
        var lease = CreateDraftLease(
            dueDayOfMonth: 1,
            gracePeriodDays: 0);

        var command = new ChangeBillingSettingsCommand(
            lease.LeaseId,
            10,
            5);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateOnly(2026, 3, 28));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        lease.BillingSettings.DueDayOfMonth.Should().Be(10);
        lease.BillingSettings.GracePeriodDays.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_Use_Today_From_DateTimeProvider_When_Changing_BillingSettings()
    {
        // Arrange
        var lease = CreateDraftLease(
            dueDayOfMonth: 1,
            gracePeriodDays: 0);

        var today = new DateTime(2026, 3, 28, 16, 30, 0);

        var command = new ChangeBillingSettingsCommand(
            lease.LeaseId,
            15,
            7);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lease.BillingSettings.DueDayOfMonth.Should().Be(15);
        lease.BillingSettings.GracePeriodDays.Should().Be(7);
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
