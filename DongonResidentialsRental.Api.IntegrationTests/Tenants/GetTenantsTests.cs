using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Application.Tenants.Queries;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Tenants;

public sealed class GetTenantsTests : IntegrationTestBase
{
    public GetTenantsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTenants_ShouldReturnOk_WithEmptyItems_WhenNoTenantsExist()
    {
        await ResetDatabaseAsync();

        var response = await Client.GetAsync("/api/tenants?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TenantResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTenants_ShouldReturnPagedTenants_WhenTenantsExist()
    {
        await ResetDatabaseAsync();

        await TenantSeederHelper.SeedTenantsAsync(
            Factory,
            TenantSeederHelper.CreateTenant("John", "Doe", "john@email.com", "09111111111"),
            TenantSeederHelper.CreateTenant("Jane", "Smith", "jane@email.com", "09222222222"),
            TenantSeederHelper.CreateTenant("Mark", "Lee", "mark@email.com", "09333333333"));

        var response = await Client.GetAsync("/api/tenants?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TenantResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTenants_ShouldFilterBySearchTerm()
    {
        await ResetDatabaseAsync();

        await TenantSeederHelper.SeedTenantsAsync(
            Factory,
            TenantSeederHelper.CreateTenant("Jayson", "Santiago", "jayson@email.com", "09111111111"),
            TenantSeederHelper.CreateTenant("Maria", "Clara", "maria@email.com", "09222222222"),
            TenantSeederHelper.CreateTenant("Pedro", "Reyes", "pedro@email.com", "09333333333"));

        var response = await Client.GetAsync("/api/tenants?searchTerm=Jayson&page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TenantResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        var tenant = result.Items.Single();
        tenant.TenantId.Should().NotBeEmpty();
        tenant.FirstName.Should().Be("Jayson");
        tenant.LastName.Should().Be("Santiago");
        tenant.Email.Should().Be("jayson@email.com");
        tenant.PhoneNumber.Should().Be("09111111111");
    }

    [Fact]
    public async Task GetTenants_ShouldRespectPageSize()
    {
        await ResetDatabaseAsync();

        var tenants = Enumerable.Range(1, 5)
            .Select(i => TenantSeederHelper.CreateTenant(
                $"Tenant{i}",
                "Test",
                $"tenant{i}@email.com",
                $"0911111111{i}"))
            .ToArray();

        await TenantSeederHelper.SeedTenantsAsync(Factory, tenants);

        var response = await Client.GetAsync("/api/tenants?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TenantResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetTenants_ShouldRespectPageNumber()
    {
        await ResetDatabaseAsync();

        var tenants = Enumerable.Range(1, 5)
            .Select(i => TenantSeederHelper.CreateTenant(
                $"Tenant{i}",
                "Test",
                $"tenant{i}@email.com",
                $"0911111111{i}"))
            .ToArray();

        await TenantSeederHelper.SeedTenantsAsync(Factory, tenants);

        var response = await Client.GetAsync("/api/tenants?page=2&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<TenantResponse>>();

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }
}
