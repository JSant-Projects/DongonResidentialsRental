using AwesomeAssertions;
using DomainLease = DongonResidentialsRental.Domain.Lease.Lease;
using DongonResidentialsRental.Domain.Meter;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using System;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared.Exceptions;

namespace DongonResidentialsRental.Domain.UnitTests.Lease;

public class LeaseSpecifications
{
    // ---------- Helpers ----------
    private static TenantId AnyTenantId() => new TenantId(Guid.NewGuid());

    private static UnitId AnyUnitId() => new UnitId(Guid.NewGuid());

    private static LeaseTerm TermStarting(DateOnly start, DateOnly? end = null)
        => LeaseTerm.Create(start, end);

    private static Money AnyMonthlyRate(decimal amount = 1000m)
        => Money.Create("CAD", amount);

    private static BillingSettings AnyBillingSettings(int dueDay = 5, int graceDays = 3)
        => BillingSettings.Create(dueDay, graceDays);

    private static UtilityResponsibility Responsibility(bool tenantPaysElectricity, bool tenantPaysWater)
        => UtilityResponsibility.Create(tenantPaysElectricity, tenantPaysWater);

    private static MeterBinding Binding(ElectricityMeterId? electricityMeterId, WaterMeterId? waterMeterId)
        => new MeterBinding(electricityMeterId, waterMeterId);

    private static DomainLease CreateDraftLease(DateOnly start, DateOnly? end = null, decimal rate = 1000m)
    {
        return DomainLease.Create(
            AnyTenantId(),
            AnyUnitId(),
            TermStarting(start, end),
            AnyMonthlyRate(rate),
            AnyBillingSettings(),
            Responsibility(true, false)
        );
    }

    private static DomainLease CreateActiveLease(
        DateOnly start,
        DateOnly? end,
        DateOnly today,
        UtilityResponsibility responsibility,
        BillingSettings? billing = null,
        decimal rate = 1000m)
    {
        var lease = DomainLease.Create(
            AnyTenantId(),
            AnyUnitId(),
            TermStarting(start, end),
            AnyMonthlyRate(rate),
            AnyBillingSettings(),
            Responsibility(true, false)
        );

        lease.ChangeUtilityResponsibility(responsibility);
        lease.ChangeBillingSettings(billing ?? AnyBillingSettings(), today);
        lease.Activate();

        lease.Status.Should().Be(LeaseStatus.Active);
        lease.IsActive(today).Should().BeTrue();

        return lease;
    }

    // ---------- Create ----------
    [Fact]
    public void Create_Should_Return_Lease_When_Arguments_Are_Valid()
    {
        // Arrange
        var tenantId = AnyTenantId();
        var unitId = AnyUnitId();
        var start = new DateOnly(2026, 01, 01);
        var term = TermStarting(start, new DateOnly(2026, 12, 31));
        var rate = AnyMonthlyRate(1200m);
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        var lease = DomainLease.Create(tenantId, unitId, term, rate, billingSettings, responsibility);

        // Assert
        lease.Should().NotBeNull();
        lease.Should().BeOfType<DomainLease>();
        lease.LeaseId.Should().NotBeNull();
        lease.Occupancy.Should().Be(tenantId);
        lease.UnitId.Should().Be(unitId);
        lease.Term.Should().Be(term);
        lease.MonthlyRate.Should().Be(rate);
        lease.Status.Should().Be(LeaseStatus.Draft);
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_Occupancy_Is_Null()
    {
        // Arrange
        TenantId occupancy = null;
        var unitId = AnyUnitId();
        var term = TermStarting(new DateOnly(2026, 01, 01));
        var rate = AnyMonthlyRate();
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        Action act = () => DomainLease.Create(occupancy, unitId, term, rate, billingSettings, responsibility);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Occupancy cannot be null*");
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_UnitId_Is_Null()
    {
        // Arrange
        var occupancy = AnyTenantId();
        UnitId unitId = null;
        var term = TermStarting(new DateOnly(2026, 01, 01));
        var rate = AnyMonthlyRate();
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        Action act = () => DomainLease.Create(occupancy, unitId, term, rate, billingSettings, responsibility);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Unit ID cannot be null*");
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_LeaseTerm_Is_Null()
    {
        // Arrange
        var occupancy = AnyTenantId();
        var unitId = AnyUnitId();
        LeaseTerm term = null;
        var rate = AnyMonthlyRate();
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        Action act = () => DomainLease.Create(occupancy, unitId, term, rate, billingSettings, responsibility);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Lease term cannot be null*");
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_MonthlyRate_Is_Null()
    {
        // Arrange
        var occupancy = AnyTenantId();
        var unitId = AnyUnitId();
        var term = TermStarting(new DateOnly(2026, 01, 01));
        Money rate = null;
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        Action act = () => DomainLease.Create(occupancy, unitId, term, rate, billingSettings, responsibility);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Monthly rate cannot be null*");
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_MonthlyRate_Is_Zero()
    {
        // Arrange
        var occupancy = AnyTenantId();
        var unitId = AnyUnitId();
        var term = TermStarting(new DateOnly(2026, 01, 01));
        Money rate = AnyMonthlyRate(0m);
        var billingSettings = AnyBillingSettings();
        var responsibility = Responsibility(true, false);

        // Act
        Action act = () => DomainLease.Create(occupancy, unitId, term, rate, billingSettings, responsibility);

        // Assert
        act.Should().ThrowExactly<DomainException>()
           .WithMessage("Monthly rate must be greater than zero.");
    }

    // ---------- Activate ----------
    [Fact]
    public void Activate_Should_Set_Status_To_Active_When_All_Required_Fields_Are_Present()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01), end: new DateOnly(2026, 12, 31));

        // Act
        lease.Activate();

        // Assert
        lease.Status.Should().Be(LeaseStatus.Active);
    }

    // ---------- ChangeLeaseTerm ----------
    [Fact]
    public void ChangeLeaseTerm_Should_Update_Term_When_Lease_Is_Draft()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01), end: new DateOnly(2026, 12, 31));
        var today = new DateOnly(2026, 01, 10);
        var newTerm = TermStarting(new DateOnly(2026, 02, 01), new DateOnly(2026, 12, 31));

        // Act
        lease.ChangeLeaseTerm(newTerm, today);

        // Assert
        lease.Term.Should().Be(newTerm);
    }

    [Fact]
    public void ChangeLeaseTerm_Should_Throw_OperationNotAllowedException_When_Changing_StartDate_While_Active()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var start = new DateOnly(2026, 01, 01);

        var lease = CreateActiveLease(
            start: start,
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false));

        var changedStart = TermStarting(new DateOnly(2026, 01, 02), new DateOnly(2026, 12, 31));

        // Act
        Action act = () => lease.ChangeLeaseTerm(changedStart, today);

        // Assert
        act.Should().ThrowExactly<OperationNotAllowedException>()
            .WithMessage("Cannot change lease start date once the lease is active.");
    }

    [Fact]
    public void ChangeLeaseTerm_Should_Throw_OperationNotAllowedException_When_Active_And_New_EndDate_Is_Past()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var start = new DateOnly(2026, 01, 01);

        var lease = CreateActiveLease(
            start: start,
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false));

        var newTerm = TermStarting(start, end: new DateOnly(2026, 01, 05)); // < today

        // Act
        Action act = () => lease.ChangeLeaseTerm(newTerm, today);

        // Assert
        act.Should().ThrowExactly<OperationNotAllowedException>()
            .WithMessage("Cannot change lease end date to a past date for an active lease.");
    }

    // ---------- ChangeBillingSettings ----------
    [Fact]
    public void ChangeBillingSettings_Should_Set_Settings_When_Lease_Is_Draft()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01));
        var today = new DateOnly(2026, 01, 02);
        var settings = AnyBillingSettings(dueDay: 10, graceDays: 5);

        // Act
        lease.ChangeBillingSettings(settings, today);

        // Assert
        lease.BillingSettings.Should().Be(settings);
    }

    [Fact]
    public void ChangeBillingSettings_Should_Throw_OperationNotAllowedException_When_Active_And_DueDay_Changes_After_DueDate_Reached()
    {
        // Arrange
        // Existing due day = 5, and today is 5th => lock kicks in
        var today = new DateOnly(2026, 01, 05);
        var start = new DateOnly(2026, 01, 01);

        var lease = CreateActiveLease(
            start: start,
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false),
            billing: AnyBillingSettings(dueDay: 5, graceDays: 3));

        var newSettings = AnyBillingSettings(dueDay: 10, graceDays: 3);

        // Act
        Action act = () => lease.ChangeBillingSettings(newSettings, today);

        // Assert
        act.Should().ThrowExactly<OperationNotAllowedException>()
            .WithMessage("Cannot change the due day once the current billing due date has been reached.");
    }

    [Fact]
    public void ChangeBillingSettings_Should_Allow_Changing_DueDay_When_Active_But_Before_DueDate()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 03); // before due day 5
        var start = new DateOnly(2026, 01, 01);

        var lease = CreateActiveLease(
            start: start,
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false),
            billing: AnyBillingSettings(dueDay: 5, graceDays: 3));

        var newSettings = AnyBillingSettings(dueDay: 10, graceDays: 3);

        // Act
        lease.ChangeBillingSettings(newSettings, today);

        // Assert
        lease.BillingSettings.Should().Be(newSettings);
    }

    // ---------- ChangeMonthlyRate ----------
    [Fact]
    public void ChangeMonthlyRate_Should_Update_Rate_When_Lease_Is_Draft()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01), rate: 1000m);
        var today = new DateOnly(2026, 01, 10);
        var newRate = AnyMonthlyRate(1500m);

        // Act
        lease.ChangeMonthlyRate(newRate, today);

        // Assert
        lease.MonthlyRate.Should().Be(newRate);
    }

    [Fact]
    public void ChangeMonthlyRate_Should_Throw_DomainException_When_NewRate_Is_Zero_Or_Less()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01));
        var today = new DateOnly(2026, 01, 10);
        var newRate = AnyMonthlyRate(0m);

        // Act
        Action act = () => lease.ChangeMonthlyRate(newRate, today);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Monthly rate must be greater than zero.");
    }

    // ---------- ChangeUtilityResponsibility ----------
    [Fact]
    public void ChangeUtilityResponsibility_Should_Set_Value_When_Draft()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01));
        var resp = Responsibility(true, true);

        // Act
        lease.ChangeUtilityResponsibility(resp);

        // Assert
        lease.UtilityResponsibility.Should().Be(resp);
    }

    [Fact]
    public void ChangeUtilityResponsibility_Should_Throw_OperationNotAllowedException_When_Not_Draft()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var lease = CreateActiveLease(
            start: new DateOnly(2026, 01, 01),
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false));

        // Act
        Action act = () => lease.ChangeUtilityResponsibility(Responsibility(false, false));

        // Assert
        act.Should().ThrowExactly<OperationNotAllowedException>()
            .WithMessage("Operation allowed only when lease is in Draft state.");
    }

    // ---------- Terminate ----------
    [Fact]
    public void Terminate_Should_Set_Status_To_Terminated_And_Update_EndDate()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var start = new DateOnly(2026, 01, 01);
        var originalEnd = new DateOnly(2026, 12, 31);

        var lease = CreateActiveLease(
            start: start,
            end: originalEnd,
            today: today,
            responsibility: Responsibility(true, false));

        var terminationDate = new DateOnly(2026, 02, 01);

        // Act
        lease.Terminate(terminationDate, today);

        // Assert
        lease.Status.Should().Be(LeaseStatus.Terminated);
        lease.Term.StartDate.Should().Be(start);
        lease.Term.EndDate.Should().Be(terminationDate);
    }

    [Fact]
    public void Terminate_Should_Throw_OperationNotAllowedException_When_Lease_Is_Not_Active()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01));
        var terminationDate = new DateOnly(2026, 01, 10);

        // Act
        Action act = () => lease.Terminate(terminationDate, today);

        // Assert
        act.Should().ThrowExactly<OperationNotAllowedException>()
            .WithMessage("Cannot terminate a lease that is not active.");
    }

    [Fact]
    public void Terminate_Should_Throw_DomainException_When_TerminationDate_Is_Later_Than_EndDate()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var start = new DateOnly(2026, 01, 01);
        var end = new DateOnly(2026, 02, 01);

        var lease = CreateActiveLease(
            start: start,
            end: end,
            today: today,
            responsibility: Responsibility(true, false));

        var terminationDate = new DateOnly(2026, 02, 02);

        // Act
        Action act = () => lease.Terminate(terminationDate, today);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Termination date cannot be later than the lease end date.");
    }

    // ---------- IsActive / IsExpired ----------
    [Fact]
    public void IsActive_Should_Return_True_When_Status_Is_Active_And_Term_Includes_Today()
    {
        // Arrange
        var today = new DateOnly(2026, 01, 10);
        var lease = CreateActiveLease(
            start: new DateOnly(2026, 01, 01),
            end: new DateOnly(2026, 12, 31),
            today: today,
            responsibility: Responsibility(true, false));

        // Act
        var result = lease.IsActive(today);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Return_True_When_Active_And_Today_Is_After_EndDate()
    {
        // Arrange
        var activationDay = new DateOnly(2026, 01, 10);
        var start = new DateOnly(2026, 01, 01);
        var end = new DateOnly(2026, 01, 15);

        var lease = CreateActiveLease(
            start: start,
            end: end,
            today: activationDay,
            responsibility: Responsibility(true, false));

        var afterEnd = new DateOnly(2026, 01, 16);

        // Act
        var expired = lease.IsExpired(afterEnd);

        // Assert
        expired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Return_False_When_Not_Active()
    {
        // Arrange
        var lease = CreateDraftLease(start: new DateOnly(2026, 01, 01), end: new DateOnly(2026, 01, 15));
        var afterEnd = new DateOnly(2026, 01, 16);

        // Act
        var expired = lease.IsExpired(afterEnd);

        // Assert
        expired.Should().BeFalse();
    }
}