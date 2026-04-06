using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class LeaseSeederHelper
{
    public static Lease CreateLease(
        TenantId tenantId,
        UnitId unitId,
        DateOnly startDate,
        DateOnly? endDate = null,
        decimal monthlyRate = 1000m,
        int dueDayOfMonth = 1,
        int gracePeriodDays = 5,
        bool tenantPaysElectricity = true,
        bool tenantPaysWater = true,
        string currency = "CAD",
        LeaseStatus status = LeaseStatus.Draft,
        DateOnly? terminationDate = null)
    {
        var term = LeaseTerm.Create(startDate, endDate);
        var money = Money.Create(currency, monthlyRate);
        var billingSettings = BillingSettings.Create(dueDayOfMonth, gracePeriodDays);
        var utilityResponsibility = UtilityResponsibility.Create(
            tenantPaysElectricity,
            tenantPaysWater);

        var lease = Lease.Create(
            tenantId,
            unitId,
            term,
            money,
            billingSettings,
            utilityResponsibility);

        switch (status)
        {
            case LeaseStatus.Active:
                lease.Activate();
                break;
            case LeaseStatus.Terminated:
                var today =  DateOnly.FromDateTime(DateTime.UtcNow);
                var terminationDateToUse = terminationDate ?? today;

                lease.Activate();
                lease.Terminate(terminationDateToUse, today);
                break;
        }

        return lease;
    }

    public static async Task SeedLeasesAsync(
        IntegrationTestWebAppFactory factory,
        params Lease[] leases)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Leases.AddRangeAsync(leases);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Lease> SeedLeaseAsync(
        IntegrationTestWebAppFactory factory,
        TenantId tenantId,
        UnitId unitId,
        DateOnly startDate,
        DateOnly? endDate = null,
        decimal monthlyRate = 1000m,
        int dueDayOfMonth = 1,
        int gracePeriodDays = 5,
        bool tenantPaysElectricity = true,
        bool tenantPaysWater = true,
        string currency = "CAD",
        LeaseStatus status = LeaseStatus.Draft,
        DateOnly? terminationDate = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lease = CreateLease(
            tenantId,
            unitId,
            startDate,
            endDate,
            monthlyRate,
            dueDayOfMonth,
            gracePeriodDays,
            tenantPaysElectricity,
            tenantPaysWater,
            currency,
            status,
            terminationDate);

        await dbContext.Leases.AddAsync(lease);
        await dbContext.SaveChangesAsync();

        return lease;
    }
}
