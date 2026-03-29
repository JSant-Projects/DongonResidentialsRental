using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.TerminateLease;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.TerminateLease;

public sealed class TerminateLeaseCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly TerminateLeaseCommandHandler _handler;

    public TerminateLeaseCommandHandlerTests()
    {
        _handler = new TerminateLeaseCommandHandler(
            _leaseRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();

        var command = new TerminateLeaseCommand(
            leaseId,
            new DateOnly(2026, 6, 30));

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
    public async Task Handle_Should_Terminate_Lease_When_Lease_Exists()
    {
        // Arrange
        var lease = CreateActiveLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        var command = new TerminateLeaseCommand(
            lease.LeaseId,
            new DateOnly(2026, 6, 30));

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateTime(2026, 6, 15, 10, 0, 0));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        lease.Status.Should().Be(LeaseStatus.Terminated);
        lease.Term.EndDate.Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var lease = CreateActiveLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        var command = new TerminateLeaseCommand(
            lease.LeaseId,
            new DateOnly(2026, 7, 31));

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateTime(2026, 7, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());
    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());

    private static Lease CreateActiveLease(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        var lease = Lease.Create(
            NewTenantId(),
            NewUnitId(),
            LeaseTerm.Create(startDate ?? new DateOnly(2026, 1, 1), endDate),
            Money.Create(currency, monthlyRentAmount),
            BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            UtilityResponsibility.Create(false, false));

        lease.Activate();

        return lease;
    }
}
