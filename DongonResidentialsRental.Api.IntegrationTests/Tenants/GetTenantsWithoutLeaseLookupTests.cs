using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Tenants.Queries;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Tenants;

public sealed class GetTenantsWithoutLeaseLookupTests : IntegrationTestBase
{
    public GetTenantsWithoutLeaseLookupTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTenantsWithoutLeaseLookup_ShouldReturnAllTenants_WhenNoLeasesExist()
    {
        await ResetDatabaseAsync();

        var tenant1 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "John",
            tenantLastName: "Doe",
            tenantEmail: "john@email.com",
            tenantPhoneNumber: "09111111111");

        var tenant2 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane@email.com",
            tenantPhoneNumber: "09222222222");

        var response = await Client.GetAsync("/api/tenants/without-lease");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<TenantLookupResponse>>();

        result.Should().NotBeNull();
        result!.Should().HaveCount(2);

        result.Should().Contain(x =>
            x.TenantId == tenant1.TenantId.Id &&
            x.FullName == "John Doe");

        result.Should().Contain(x =>
            x.TenantId == tenant2.TenantId.Id &&
            x.FullName == "Jane Smith");
    }

    [Fact]
    public async Task GetTenantsWithoutLeaseLookup_ShouldExcludeTenant_WithActiveLease()
    {
        await ResetDatabaseAsync();

        var tenantWithLease = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jayson",
            tenantLastName: "Santiago",
            tenantEmail: "jayson@email.com",
            tenantPhoneNumber: "09111111111");

        var tenantWithoutLease = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Maria",
            tenantLastName: "Clara",
            tenantEmail: "maria@email.com",
            tenantPhoneNumber: "09222222222");

        var unit = await UnitSeederHelper.SeedUnitAsync(Factory, unitNumber: "101");

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenantWithLease.TenantId,
            unit.UnitId,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10),
            endDate: null);

        var response = await Client.GetAsync("/api/tenants/without-lease");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<TenantLookupResponse>>();

        result.Should().NotBeNull();
        result!.Should().HaveCount(1);

        result.Should().ContainSingle(x =>
            x.TenantId == tenantWithoutLease.TenantId.Id &&
            x.FullName == "Maria Clara");

        result.Should().NotContain(x => x.TenantId == tenantWithLease.TenantId.Id);
    }

    [Fact]
    public async Task GetTenantsWithoutLeaseLookup_ShouldIncludeTenant_WhenLeaseAlreadyEnded()
    {
        await ResetDatabaseAsync();

        var tenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Pedro",
            tenantLastName: "Reyes",
            tenantEmail: "pedro@email.com",
            tenantPhoneNumber: "09333333333");

        var unit = await UnitSeederHelper.SeedUnitAsync(Factory, unitNumber: "102");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: today.AddMonths(-3),
            endDate: today.AddDays(-1));

        var response = await Client.GetAsync("/api/tenants/without-lease");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<TenantLookupResponse>>();

        result.Should().NotBeNull();
        result!.Should().ContainSingle(x =>
            x.TenantId == tenant.TenantId.Id &&
            x.FullName == "Pedro Reyes");
    }
}
