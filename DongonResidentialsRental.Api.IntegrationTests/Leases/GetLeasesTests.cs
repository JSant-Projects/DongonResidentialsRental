using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Leases.Queries;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Leases;

public sealed class GetLeasesTests : IntegrationTestBase
{
    public GetLeasesTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLeases_Should_Return_Ok_With_Leases()
    {
        // Arrange

        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Sunrise Apartments");

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "101");

        var tenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "John",
            tenantLastName: "Doe",
            tenantEmail: "john.doe@email.com");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        // Act
        var response = await Client.GetAsync("/api/leases");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        var lease = result.Items.Single();

        lease.TenantFullName.Should().Be("John Doe");
        lease.BuildingName.Should().Be("Sunrise Apartments");
        lease.UnitNumber.Should().Be("101");
        lease.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLeases_Should_Return_Empty_Result_When_No_Leases_Exist()
    {
        // Act
        var response = await Client.GetAsync("/api/leases");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLeases_Should_Filter_By_BuildingId()
    {
        // Arrange

        await ResetDatabaseAsync();

        var buildingA = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Building A");

        var buildingB = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Building B");

        var tenantA = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "John",
            tenantLastName: "Doe",
            tenantEmail: "john.doe@email.com");

        var tenantB = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@email.com");

        var unitA = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingA.BuildingId,
            unitNumber: "101");

        var unitB = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingB.BuildingId,
            unitNumber: "201");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantA.TenantId,
            unitA.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantB.TenantId,
            unitB.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        // Act
        var response = await Client.GetAsync($"api/leases?buildingId={buildingA.BuildingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        result.Items.Single().BuildingName.Should().Be("Building A");
        result.Items.Single().UnitNumber.Should().Be("101");
    }

    [Fact]
    public async Task GetLeases_Should_Filter_By_SearchTerm_Matching_Tenant_Name()
    {
        // Arrange

        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Search Building");

        var unitA = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "101");

        var unitB = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var tenantA = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "John",
            tenantLastName: "Doe",
            tenantEmail: "john.doe@email.com");

        var tenantB = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Maria",
            tenantLastName: "Santos",
            tenantEmail: "maria.santos@email.com");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantA.TenantId,
            unitA.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantB.TenantId,
            unitB.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        // Act
        var response = await Client.GetAsync("/api/leases?searchTerm=Maria");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        result.Items.Single().TenantFullName.Should().Be("Maria Santos");
    }

    [Fact]
    public async Task GetLeases_Should_Filter_By_SearchTerm_Matching_Building_Name()
    {
        // Arrange

        await ResetDatabaseAsync();

        var buildingA = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Sunrise Apartments");

        var buildingB = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Moonlight Residences");

        var tenantA = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "John",
            tenantLastName: "Doe",
            tenantEmail: "john.doe@email.com");

        var tenantB = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@email.com");

        var unitA = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingA.BuildingId,
            unitNumber: "101");

        var unitB = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            buildingB.BuildingId,
            unitNumber: "201");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantA.TenantId,
            unitA.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantB.TenantId,
            unitB.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        // Act
        var response = await Client.GetAsync("/api/leases?searchTerm=Moonlight");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        result.Items.Single().BuildingName.Should().Be("Moonlight Residences");
    }

    [Fact]
    public async Task GetLeases_Should_Filter_Active_Leases()
    {
        // Arrange

        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Active Filter Building");

        var activeUnit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "101");

        var terminatedUnit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var activeTenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Active",
            tenantLastName: "Tenant",
            tenantEmail: "active.tenant@email.com");

        var terminatedTenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Terminated",
            tenantLastName: "Tenant",
            tenantEmail: "terminated.tenant@email.com");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            activeTenant.TenantId,
            activeUnit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Active);

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            terminatedTenant.TenantId,
            terminatedUnit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            status: LeaseStatus.Terminated,
            terminationDate: new DateOnly(2026, 2, 1));

        // Act
        var response = await Client.GetAsync("/api/leases?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        result.Items.Single().TenantFullName.Should().Be("Active Tenant");
        result.Items.Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLeases_Should_Apply_Pagination()
    {
        // Arrange

        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(
            Factory,
            name: "Pagination Building");

        for (var i = 1; i <= 3; i++)
        {
            var tenant = await TenantSeederHelper.SeedTenantAsync(
                Factory,
                tenantFirstName: $"Tenant{i}",
                tenantLastName: "Test",
                tenantEmail: $"tenant{i}@email.com");

            var unit = await UnitSeederHelper.SeedUnitAsync(
                Factory,
                building.BuildingId,
                unitNumber: $"10{i}");

            await LeaseSeederHelper.SeedLeaseAsync(
                Factory,
                tenant.TenantId,
                unit.UnitId,
                startDate: new DateOnly(2026, 1, 1),
                status: LeaseStatus.Active);
        }

        // Act
        var response = await Client.GetAsync("/api/leases?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<LeaseResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(3);
    }
}
