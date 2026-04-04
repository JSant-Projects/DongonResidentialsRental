using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Application.Units.Queries;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Units;

public sealed class GetUnitsTests : IntegrationTestBase
{
    public GetUnitsTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUnits_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        await ResetDatabaseAsync();
        await UnitSeederHelper.SeedUnitAsync(Factory);

        // Act
        var response = await Client.GetAsync("/api/units");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUnits_ShouldReturnAllUnits_WhenNoFiltersAreProvided()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory, name: "Building A");

        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "101", floor: 1);
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "102", floor: 1);
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "201", floor: 2);

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<UnitResponse>>("/api/units");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetUnits_ShouldFilterByStatus()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory, name: "Building A");

        await UnitSeederHelper.CreateUnitAsync(Factory, buildingId: building.BuildingId, status: UnitStatus.Active, unitNumber: "101");
        await UnitSeederHelper.CreateUnitAsync(Factory, buildingId: building.BuildingId, UnitStatus.Inactive, unitNumber: "102");
        await UnitSeederHelper.CreateUnitAsync(Factory, buildingId: building.BuildingId, UnitStatus.Maintenance, unitNumber: "103");

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<UnitResponse>>(
            "/api/units?status=Inactive");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].UnitNumber.Should().Be("102");
    }

    [Fact]
    public async Task GetUnits_ShouldFilterByUnitNumber()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory, name: "Building A");

        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "101");
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "102");
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "201");

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<UnitResponse>>(
            "/api/units?unitNumber=102");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].UnitNumber.Should().Be("102");
    }

    [Fact]
    public async Task GetUnits_ShouldFilterByFloor()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory, name: "Building A");

        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "101", floor: 1);
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "102", floor: 1);
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "201", floor: 2);

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<UnitResponse>>(
            "/api/units?floor=2");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].UnitNumber.Should().Be("201");
        result.Items[0].Floor.Should().Be(2);
    }

    [Fact]
    public async Task GetUnits_ShouldRespectPaging()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory, name: "Building A");

        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "101");
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "102");
        await UnitSeederHelper.SeedUnitAsync(Factory, buildingId: building.BuildingId, unitNumber: "103");

        // Act
        var result = await Client.GetFromJsonAsync<PagedResult<UnitResponse>>(
            "/api/units?page=1&pageSize=2");

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);
    }
}
