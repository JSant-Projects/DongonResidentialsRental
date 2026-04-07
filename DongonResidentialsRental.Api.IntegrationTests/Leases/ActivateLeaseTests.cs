using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Leases;

public sealed class ActivateLeaseTests : IntegrationTestBase
{
    public ActivateLeaseTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ActivateLease_ShouldReturnNoContent_WhenLeaseExists()
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
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 3, 31),
            status: LeaseStatus.Draft);

        var response = await Client.PutAsync($"/api/leases/{lease.LeaseId.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ActivateLease_ShouldReturnNotFound_WhenLeaseDoesNotExist()
    {
        await ResetDatabaseAsync();

        var leaseId = Guid.NewGuid();

        var response = await Client.PutAsync($"/api/leases/{leaseId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateLease_ShouldReturnConflict_WhenLeaseIsAlreadyActive()
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
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 3, 31),
            status: LeaseStatus.Active);

        var response = await Client.PutAsync($"/api/leases/{lease.LeaseId.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
