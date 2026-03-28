using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.ChangeUtilityResponsibility;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.ChangeUtilityResponsibility;

public sealed class ChangeUtilityResponsibilityCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();

    private readonly ChangeUtilityResponsibilityCommandHandler _handler;

    public ChangeUtilityResponsibilityCommandHandlerTests()
    {
        _handler = new ChangeUtilityResponsibilityCommandHandler(_leaseRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();

        var command = new ChangeUtilityResponsibilityCommand(
            leaseId,
            TenantPaysElectricity: true,
            TenantPaysWater: false);

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
    public async Task Handle_Should_Change_UtilityResponsibility_When_Lease_Exists()
    {
        // Arrange
        var lease = CreateDraftLease(
            tenantPaysElectricity: false,
            tenantPaysWater: false);

        var command = new ChangeUtilityResponsibilityCommand(
            lease.LeaseId,
            TenantPaysElectricity: true,
            TenantPaysWater: false);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        lease.UtilityResponsibility.TenantPaysElectricity.Should().BeTrue();
        lease.UtilityResponsibility.TenantPaysWater.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var lease = CreateDraftLease(
            tenantPaysElectricity: false,
            tenantPaysWater: false);

        var command = new ChangeUtilityResponsibilityCommand(
            lease.LeaseId,
            TenantPaysElectricity: true,
            TenantPaysWater: true);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

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
        int gracePeriodDays = 0,
        bool tenantPaysElectricity = false,
        bool tenantPaysWater = false)
    {
        return Lease.Create(
            NewTenantId(),
            NewUnitId(),
            LeaseTerm.Create(startDate ?? new DateOnly(2026, 4, 1), endDate),
            Money.Create(currency, monthlyRentAmount),
            BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            UtilityResponsibility.Create(tenantPaysElectricity, tenantPaysWater));
    }
}
