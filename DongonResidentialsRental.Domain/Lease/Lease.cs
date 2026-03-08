using DongonResidentialsRental.Domain.Lease.Events;
using DongonResidentialsRental.Domain.Meter;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Lease;

public sealed class Lease: AggregateRoot
{
    public LeaseId LeaseId { get; }
    public TenantId Occupancy { get; }
    public UnitId UnitId { get; }
    public Money MonthlyRate { get; private set; }
    public LeaseTerm Term { get; private set; }
    public BillingSettings? BillingSettings { get; private set; }
    public UtilityResponsibility? UtilityResponsibility { get; private set; }
    public MeterBinding? MeterBinding { get; private set; }
    public LeaseStatus Status { get; private set; }

    private Lease() { }
    
    private Lease(TenantId occupancy, UnitId unitId, LeaseTerm leaseTerm, Money monthlyRate)
    {
        LeaseId = new LeaseId(Guid.NewGuid());
        Occupancy = occupancy;
        UnitId = unitId;
        Term = leaseTerm;
        MonthlyRate = monthlyRate;
        Status = LeaseStatus.Draft;
    }

    public static Lease Create(TenantId occupancy, UnitId unitId, LeaseTerm leaseTerm, Money monthlyRate)
    {
        Ensure.NotNull(occupancy, "Occupancy cannot be null");
        Ensure.NotNull(unitId, "Unit ID cannot be null");
        Ensure.NotNull(leaseTerm, "Lease term cannot be null");
        Ensure.NotNull(monthlyRate, "Monthly rate cannot be null");

        if (monthlyRate.Amount <= 0)
            throw new DomainException("Monthly rate must be greater than zero.");
        
        var lease = new Lease(occupancy, unitId, leaseTerm, monthlyRate);

        // Add domain event for lease creation
        lease.AddDomainEvent(
            new LeaseCreatedDomainEvent(
                lease.LeaseId, 
                occupancy, 
                unitId, 
                leaseTerm.StartDate));

        return lease;
    }

    public void Activate()
    {
        EnsureIsDraft();
        EnsureAllRequiredFieldsPresent();
        Status = LeaseStatus.Active;

        // Add domain event for lease activation
        AddDomainEvent(new LeaseActivatedDomainEvent(LeaseId, Occupancy, UnitId));   
    }

    private void EnsureAllRequiredFieldsPresent()
    {
        Ensure.NotNull(BillingSettings, "Billing settings must be set before activating the lease.");
        Ensure.NotNull(UtilityResponsibility, "Utility responsibility must be set before activating the lease.");
        Ensure.NotNull(MeterBinding, "Meter binding must be set before activating the lease.");
    }

    public void ChangeLeaseTerm(LeaseTerm newTerm, DateOnly today)
    {
        Ensure.NotNull(newTerm, "New lease term cannot be null");
        EnsureIsDraftOrActive(today);

        // If lease is Active, we generally treat the start date as immutable.
        if (IsActive(today) && Term.StartDate != newTerm.StartDate)
            throw new DomainException("Cannot change lease start date once the lease is active.");

        // If lease is Active, don't allow retroactive end dates.
        if (IsActive(today) && (newTerm.EndDate.HasValue && newTerm.EndDate < today))
            throw new DomainException("Cannot change lease end date to a past date for an active lease.");

        Term = newTerm;
    }

    public void ChangeBillingSettings(BillingSettings newSettings, DateOnly today)
    {
        Ensure.NotNull(newSettings, "New billing settings cannot be null");
        EnsureIsDraftOrActive(today);

        // If the lease is active *today*, prevent billing-cycle breaking changes.
        if (IsActive(today) && BillingSettings is not null)
        {
            // Don't allow changing due day once we've reached the "billing cutover" for this cycle.
            // Simple rule: once we are in the month of the due day, lock the due day.
            var currentDueDate = new DateOnly(today.Year, today.Month, BillingSettings.DueDayOfMonth);

            if (today >= currentDueDate && newSettings.DueDayOfMonth != BillingSettings.DueDayOfMonth)
            {
                throw new DomainException(
                    "Cannot change the due day once the current billing due date has been reached."
                );
            }
        }

        BillingSettings = newSettings;
    }

    public void BindMeters(MeterBinding meterBinding)
    {
        EnsureIsDraft();
        Ensure.NotNull(UtilityResponsibility, "Utility responsibility must be set before binding meters.");

        if (meterBinding.ElectricityMeterId is null && meterBinding.WaterMeterId is null)
            throw new DomainException("At least one meter (electricity or water) must be bound to the lease.");

        if (meterBinding.ElectricityMeterId is not null 
            && meterBinding.WaterMeterId is not null 
            && meterBinding.ElectricityMeterId.Id == meterBinding.WaterMeterId.Id)
            throw new DomainException("Electricity and water meters cannot be the same.");

        if (UtilityResponsibility!.TenantPaysElectricity 
            && meterBinding.ElectricityMeterId is null)
            throw new DomainException("Electricity meter must be bound if tenant is responsible for electricity.");

        if (UtilityResponsibility!.TenantPaysWater 
            && meterBinding.WaterMeterId is null)
            throw new DomainException("Water meter must be bound if tenant is responsible for water.");

        MeterBinding = meterBinding;
    }

    public void ChangeUtilityResponsibility(UtilityResponsibility responsibility)
    {
        EnsureIsDraft();
        UtilityResponsibility = responsibility;
    }

    private void EnsureIsDraftOrActive(DateOnly dateNow)
    {
        if (Status is LeaseStatus.Draft)
            return;

        if (IsActive(dateNow))
            return;
            
        throw new DomainException("Operation allowed only when lease is in Draft or Active state.");
    }

    private void EnsureIsDraft()
    {
        if (Status is LeaseStatus.Draft)
            return;

        throw new DomainException("Operation allowed only when lease is in Draft state.");
    }

    public void ChangeMonthlyRate(Money newRate, DateOnly today)
    {
        Ensure.NotNull(newRate, "New monthly rate cannot be null");
        if (newRate.Amount <= 0)
            throw new DomainException("Monthly rate must be greater than zero.");

        EnsureIsDraftOrActive(today);
        MonthlyRate = newRate;
    }

    public void Terminate(DateOnly terminationDate, DateOnly today)
    {
        if (!IsActive(today))
            throw new DomainException("Cannot terminate a lease that is not active.");

        if (Term.EndDate.HasValue && terminationDate > Term.EndDate.Value)
            throw new DomainException("Termination date cannot be later than the lease end date.");

        Term = LeaseTerm.Create(Term.StartDate, terminationDate);


        Status = LeaseStatus.Terminated;

        // Add domain event for lease termination
        AddDomainEvent(new LeaseTerminatedDomainEvent(LeaseId, Occupancy, UnitId, terminationDate));
    }

    private void EnsureIsActive(DateOnly dateNow)
    {
        if (IsActive(dateNow))
            return;

        throw new DomainException("Operation allowed only when lease is active.");
    }

    public bool IsActive(DateOnly dateNow) => Status == LeaseStatus.Active && Term.Includes(dateNow);

    public bool IsExpired(DateOnly today)
    {
        return Status == LeaseStatus.Active
            && Term.EndDate is not null
            && today > Term.EndDate.Value;
    }

}
