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

public sealed class ChangeMonthlyRateTests : IntegrationTestBase
{
    public ChangeMonthlyRateTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeMonthlyRate_ShouldReturnNoContent_WhenRequestIsValid()
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
            monthlyRate: 1200m,
            currency: "CAD",
            status: LeaseStatus.Active);

        var request = new
        {
            NewMonthlyRate = 1500m,
            Currency = "CAD"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/monthly-rate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeMonthlyRate_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            NewMonthlyRate = 0m,
            Currency = ""
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/monthly-rate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeMonthlyRate_ShouldReturnNotFound_WhenLeaseDoesNotExist()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            NewMonthlyRate = 1500m,
            Currency = "CAD"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{Guid.NewGuid()}/monthly-rate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeMonthlyRate_ShouldReturnConflict_WhenLeaseIsTerminated()
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
            monthlyRate: 1200m,
            currency: "CAD",
            status: LeaseStatus.Terminated,
            terminationDate: today);

        var request = new
        {
            NewMonthlyRate = 1500m,
            Currency = "CAD"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/leases/{lease.LeaseId.Id}/monthly-rate",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}