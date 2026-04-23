using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Leases;

public sealed class ChangeBillingSettingsTests : IntegrationTestBase
{
    public ChangeBillingSettingsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeBillingSettings_ShouldReturnNoContent_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        Factory.Today = new DateOnly(2026, 4, 10);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 3, 31),
            dueDayOfMonth: 15,
            gracePeriodDays: 5,
            status: LeaseStatus.Active);

        var request = new
        {
            NewDueDayOfMonth = 10,
            NewGracePeriodDays = 7
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/billing-settings",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeBillingSettings_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var leaseId = Guid.NewGuid();

        var request = new
        {
            NewDueDayOfMonth = 0,
            NewGracePeriodDays = -1
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{leaseId}/billing-settings",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeBillingSettings_ShouldReturnNotFound_WhenLeaseDoesNotExist()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            NewDueDayOfMonth = 10,
            NewGracePeriodDays = 7
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/billing-settings",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeBillingSettings_ShouldReturnConflict_WhenLeaseIsTerminated()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: today.AddMonths(-3),
            endDate: today.AddMonths(3),
            status: LeaseStatus.Terminated,
            terminationDate: today);

        var request = new
        {
            NewDueDayOfMonth = 10,
            NewGracePeriodDays = 7
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/billing-settings",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
