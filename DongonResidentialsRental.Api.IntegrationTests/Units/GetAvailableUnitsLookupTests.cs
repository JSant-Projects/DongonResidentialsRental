using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Units.Queries;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Units;

public sealed class GetAvailableUnitsLookupTests : IntegrationTestBase
{
    public GetAvailableUnitsLookupTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAvailableUnitsLookup_ShouldReturnBadRequest_WhenBuildingIdIsMissing()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/api/units/available");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("BuildingId is required.");
    }

    [Fact]
    public async Task GetAvailableUnitsLookup_ShouldReturnBadRequest_WhenBuildingIdIsEmpty()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"/api/units/available?buildingId={Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("BuildingId is required.");
    }

    [Fact]
    public async Task GetAvailableUnitsLookup_ShouldReturnOnlyActiveUnitsWithoutActiveLease_ForTheSpecifiedBuilding()
    {
        // Arrange
        await ResetDatabaseAsync();

        Unit? availableUnit;
        Unit? inactiveUnit;
        Unit? otherBuildingUnit;

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var building1 = Building.Create(
                "Building A",
                Address.Create("123 Main St", "Camrose", "AB", "T4V 1A1"));

            var building2 = Building.Create(
                "Building B",
                Address.Create("456 Main St", "Camrose", "AB", "T4V 1A2"));

            availableUnit = Unit.Create(building1.BuildingId, "101", 1);

            inactiveUnit = Unit.Create(building1.BuildingId, "102", 1);
            inactiveUnit.Deactivate();

            otherBuildingUnit = Unit.Create(building2.BuildingId, "201", 2);

            await dbContext.Buildings.AddRangeAsync(building1, building2);
            await dbContext.Units.AddRangeAsync(availableUnit, inactiveUnit, otherBuildingUnit);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var result = await Client.GetFromJsonAsync<List<UnitLookupResponse>>(
            $"/api/units/available?buildingId={availableUnit!.BuildingId}");

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);

        result[0].UnitId.Should().Be(availableUnit.UnitId.Id);
        result[0].UnitNumber.Should().Be("101");
    }

    [Fact]
    public async Task GetAvailableUnitsLookup_ShouldExcludeUnitsWithActiveLease()
    {
        // Arrange
        await ResetDatabaseAsync();

        Unit unitWithoutLease;
        Unit unitWithActiveLease;
        Tenant tenant;

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var building = Building.Create(
                "Test Building",
                Address.Create("123 Main St", "Camrose", "AB", "T4V 1A1"));

            unitWithoutLease = Unit.Create(building.BuildingId, "101", 1);
            unitWithActiveLease = Unit.Create(building.BuildingId, "102", 1);

            tenant = TenantSeederHelper.CreateTenant(
                "John",
                "Doe",
                "john.doe@email.com",
                "09123456789");

            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddRangeAsync(unitWithoutLease, unitWithActiveLease);
            await dbContext.Tenants.AddAsync(tenant);
            await dbContext.SaveChangesAsync();
        }

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unitWithActiveLease.UnitId,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-5)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6)));

        // Act
        var result = await Client.GetFromJsonAsync<List<UnitLookupResponse>>(
            $"/api/units/available?buildingId={unitWithoutLease.BuildingId}");

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);

        result[0].UnitId.Should().Be(unitWithoutLease.UnitId.Id);
        result[0].UnitNumber.Should().Be("101");
    }

    [Fact]
    public async Task GetAvailableUnitsLookup_ShouldIncludeUnitsWithoutLease()
    {
        // Arrange
        await ResetDatabaseAsync();

        Unit unit;

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var building = Building.Create(
                "Test Building",
                Address.Create("123 Main St", "Camrose", "AB", "T4V 1A1"));

            unit = Unit.Create(building.BuildingId, "101", 1);

            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddAsync(unit);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var result = await Client.GetFromJsonAsync<List<UnitLookupResponse>>(
            $"/api/units/available?buildingId={unit.BuildingId}");

        // Assert
        result.Should().NotBeNull();
        result!.Should().ContainSingle();

        result[0].UnitId.Should().Be(unit.UnitId.Id);
        result[0].UnitNumber.Should().Be("101");
    }
}
