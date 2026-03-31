using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.ChangeLeaseTerm;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.ChangeLeaseTerm;

public sealed class ChangeLeaseTermCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ChangeLeaseTermCommandHandler _handler;

    public ChangeLeaseTermCommandHandlerTests()
    {
        _handler = new ChangeLeaseTermCommandHandler(
            _leaseRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();

        var command = new ChangeLeaseTermCommand(
            leaseId,
            new DateOnly(2026, 5, 1),
            new DateOnly(2027, 4, 30));

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
    public async Task Handle_Should_Change_LeaseTerm_When_Lease_Exists()
    {
        // Arrange
        var lease = CreateDraftLease(
            startDate: new DateOnly(2026, 4, 1),
            endDate: new DateOnly(2027, 3, 31));

        var command = new ChangeLeaseTermCommand(
            lease.LeaseId,
            new DateOnly(2026, 5, 1),
            new DateOnly(2027, 4, 30));

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateOnly(2026, 3, 28));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        lease.Term.StartDate.Should().Be(new DateOnly(2026, 5, 1));
        lease.Term.EndDate.Should().Be(new DateOnly(2027, 4, 30));
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var lease = CreateDraftLease(
            startDate: new DateOnly(2026, 4, 1),
            endDate: new DateOnly(2027, 3, 31));

        var command = new ChangeLeaseTermCommand(
            lease.LeaseId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2027, 5, 31));

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _dateTimeProvider.Today.Returns(new DateOnly(2026, 3, 28));

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
