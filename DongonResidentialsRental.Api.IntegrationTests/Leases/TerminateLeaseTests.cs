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

public sealed class TerminateLeaseTests : IntegrationTestBase
{
    public TerminateLeaseTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task TerminateLease_ShouldReturnNoContent_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        Factory.Today = new DateOnly(2026, 4, 10);

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2026, 12, 31),
            status: LeaseStatus.Active);

        var request = new
        {
            TerminationDate = new DateOnly(2026, 6, 1)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/terminate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TerminateLease_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            TerminationDate = default(DateOnly)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/terminate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TerminateLease_ShouldReturnNotFound_WhenLeaseDoesNotExist()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            TerminationDate = new DateOnly(2026, 6, 1)
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/terminate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TerminateLease_ShouldReturnConflict_WhenLeaseIsAlreadyTerminated()
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
            startDate: today.AddMonths(-6),
            endDate: today.AddMonths(6),
            status: LeaseStatus.Terminated,
            terminationDate: today);

        var request = new
        {
            TerminationDate = today
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/terminate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
