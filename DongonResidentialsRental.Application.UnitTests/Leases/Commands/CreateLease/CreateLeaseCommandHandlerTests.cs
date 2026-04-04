using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Leases.Commands.CreateLease;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Leases.Commands.CreateLease;

public sealed class CreateLeaseCommandHandlerTests
{
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUnitRepository _unitRepository = Substitute.For<IUnitRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();

    private readonly CreateLeaseCommandHandler _handler;

    public CreateLeaseCommandHandlerTests()
    {
        _handler = new CreateLeaseCommandHandler(
            _leaseRepository,
            _dateTimeProvider,
            _unitRepository,
            _tenantRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_Unit_Has_Active_Lease()
    {
        // Arrange
        var today = new DateTime(2026, 3, 28, 10, 0, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var command = CreateCommand();

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _unitRepository
            .ExistsAsync(
                command.UnitId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _tenantRepository
            .ExistsAsync(
                command.Occupancy,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                command.UnitId,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Unit ({command.UnitId}) has an active lease. CreateLease Operation is not allowed");

        await _leaseRepository.DidNotReceive()
            .ExistsActiveLeaseForTenantAsync(
                Arg.Any<TenantId>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>());

        _leaseRepository.DidNotReceive()
            .Add(Arg.Any<Lease>());
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_Tenant_Has_Active_Lease()
    {
        // Arrange
        var today = new DateTime(2026, 3, 28, 10, 0, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var command = CreateCommand();

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _unitRepository
            .ExistsAsync(
                command.UnitId, 
                Arg.Any<CancellationToken>())
            .Returns(true);

        _tenantRepository
            .ExistsAsync(
                command.Occupancy,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                command.UnitId,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _leaseRepository
            .ExistsActiveLeaseForTenantAsync(
                command.Occupancy,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Tenant ({command.Occupancy}) has an active lease. CreateLease Operation is not allowed");

        _leaseRepository.DidNotReceive()
            .Add(Arg.Any<Lease>());
    }

    [Fact]
    public async Task Handle_Should_Create_Lease_Add_To_Repository_And_Return_LeaseId_When_Request_Is_Valid()
    {
        // Arrange
        var today = new DateTime(2026, 3, 28, 10, 0, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var command = CreateCommand(
            occupancy: NewTenantId(),
            unitId: NewUnitId(),
            startDate: new DateOnly(2026, 4, 1),
            endDate: null,
            currency: "CAD",
            monthlyRate: 1200m,
            dueDayOfMonth: 5,
            gracePeriodDays: 3,
            tenantPaysElectricity: true,
            tenantPaysWater: false);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _unitRepository
            .ExistsAsync(
                command.UnitId, 
                Arg.Any<CancellationToken>())
            .Returns(true);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                command.UnitId,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _tenantRepository
            .ExistsAsync(
                command.Occupancy,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _leaseRepository
            .ExistsActiveLeaseForTenantAsync(
                command.Occupancy,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(false);

        Lease? addedLease = null;

        _leaseRepository
            .When(x => x.Add(Arg.Any<Lease>()))
            .Do(callInfo => addedLease = callInfo.Arg<Lease>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedLease.Should().NotBeNull();
        addedLease!.LeaseId.Should().Be(result);
        addedLease.UnitId.Should().Be(command.UnitId);
        addedLease.Occupancy.Should().Be(command.Occupancy);

        addedLease.Term.StartDate.Should().Be(new DateOnly(2026, 4, 1));
        addedLease.Term.EndDate.Should().BeNull();

        addedLease.MonthlyRate.Currency.Should().Be("CAD");
        addedLease.MonthlyRate.Amount.Should().Be(1200m);

        addedLease.BillingSettings.DueDayOfMonth.Should().Be(5);
        addedLease.BillingSettings.GracePeriodDays.Should().Be(3);

        addedLease.UtilityResponsibility.TenantPaysElectricity.Should().BeTrue();
        addedLease.UtilityResponsibility.TenantPaysWater.Should().BeFalse();

        _leaseRepository.Received(1).Add(Arg.Any<Lease>());
    }

    [Fact]
    public async Task Handle_Should_Use_Today_From_DateTimeProvider_For_Active_Lease_Checks()
    {
        // Arrange
        var today = new DateTime(2026, 3, 28, 16, 30, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var command = CreateCommand();

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _unitRepository
            .ExistsAsync(
                command.UnitId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _tenantRepository
            .ExistsAsync(
                command.Occupancy,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _leaseRepository
            .ExistsActiveLeaseForUnitAsync(
                command.UnitId,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _leaseRepository
            .ExistsActiveLeaseForTenantAsync(
                command.Occupancy,
                expectedDate,
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _leaseRepository.Received(1)
            .ExistsActiveLeaseForUnitAsync(
                command.UnitId,
                expectedDate,
                Arg.Any<CancellationToken>());

        await _leaseRepository.Received(1)
            .ExistsActiveLeaseForTenantAsync(
                command.Occupancy,
                expectedDate,
                Arg.Any<CancellationToken>());
    }

    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());
    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());

    private static CreateLeaseCommand CreateCommand(
        TenantId? occupancy = null,
        UnitId? unitId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string currency = "CAD",
        decimal monthlyRate = 1200m,
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0,
        bool tenantPaysElectricity = false,
        bool tenantPaysWater = false)
    {
        return new CreateLeaseCommand(
            Occupancy: occupancy ?? NewTenantId(),
            UnitId: unitId ?? NewUnitId(),
            StartDate: startDate ?? new DateOnly(2026, 4, 1),
            EndDate: endDate,
            Currency: currency,
            MonthlyRate: monthlyRate,
            DueDayOfMonth: dueDayOfMonth,
            GracePeridoDays: gracePeriodDays,
            TenantPaysElectricity: tenantPaysElectricity,
            TenantPaysWater: tenantPaysWater);
    }
}
