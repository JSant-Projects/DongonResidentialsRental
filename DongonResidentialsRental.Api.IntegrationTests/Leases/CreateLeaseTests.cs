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

public sealed class CreateLeaseTests : IntegrationTestBase
{
    public CreateLeaseTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLease_ShouldReturnCreated_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var request = new
        {
            Occupancy = tenant.TenantId.Id,
            UnitId = unit.UnitId.Id,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            MonthlyRate = 1200m,
            DueDayOfMonth = 5,
            GracePeridoDays = 3,
            TenantPaysElectricity = true,
            TenantPaysWater = false,
            Currency = "CAD"
        };

        var response = await Client.PostAsJsonAsync("/api/leases", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateLease_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            Occupancy = Guid.Empty,
            UnitId = Guid.Empty,
            StartDate = default(DateOnly),
            EndDate = (DateOnly?)null,
            MonthlyRate = 0m,
            DueDayOfMonth = 0,
            GracePeridoDays = -1,
            TenantPaysElectricity = true,
            TenantPaysWater = true,
            Currency = ""
        };

        var response = await Client.PostAsJsonAsync("/api/leases", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateLease_ShouldReturnNotFound_WhenTenantDoesNotExist()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var request = new
        {
            Occupancy = Guid.NewGuid(),
            UnitId = unit.UnitId.Id,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            MonthlyRate = 1200m,
            DueDayOfMonth = 5,
            GracePeridoDays = 3,
            TenantPaysElectricity = true,
            TenantPaysWater = false,
            Currency = "CAD"
        };

        var response = await Client.PostAsJsonAsync("/api/leases", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateLease_ShouldReturnConflict_WhenUnitAlreadyHasActiveLease()
    {
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingId: building.BuildingId);

        var existingTenant = await TenantSeederHelper.SeedTenantAsync(Factory);
        var newTenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@email.com",
            tenantPhoneNumber: "09987654321");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            existingTenant.TenantId,
            unit.UnitId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            status: LeaseStatus.Active);

        var request = new
        {
            Occupancy = newTenant.TenantId.Id,
            UnitId = unit.UnitId.Id,
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            MonthlyRate = 1200m,
            DueDayOfMonth = 5,
            GracePeridoDays = 3,
            TenantPaysElectricity = true,
            TenantPaysWater = false,
            Currency = "CAD"
        };

        var response = await Client.PostAsJsonAsync("/api/leases", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
