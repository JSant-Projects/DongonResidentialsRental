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

public sealed class ChangeUtilityResponsibilityTests : IntegrationTestBase
{
    public ChangeUtilityResponsibilityTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeUtilityResponsibility_ShouldReturnNoContent_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 4, 1),
            endDate: new DateOnly(2027, 3, 31),
            tenantPaysElectricity: true,
            tenantPaysWater: true);

        var request = new
        {
            TenantPaysElectricity = false,
            TenantPaysWater = true
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId}/utility-responsibility",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeUtilityResponsibility_ShouldReturnNotFound_WhenLeaseDoesNotExist()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            TenantPaysElectricity = false,
            TenantPaysWater = true
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/utility-responsibility",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeUtilityResponsibility_ShouldReturnConflict_WhenLeaseIsTerminated()
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
            tenantPaysElectricity: true,
            tenantPaysWater: true,
            status: LeaseStatus.Terminated,
            terminationDate: today);

        var request = new
        {
            TenantPaysElectricity = false,
            TenantPaysWater = true
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/utility-responsibility",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
