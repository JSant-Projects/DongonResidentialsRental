using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Tenants;

public sealed class ChangeTenantNameTests : IntegrationTestBase
{
    public ChangeTenantNameTests(IntegrationTestWebAppFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeTenantContactInfo_ShouldReturnNoContent_AndUpdateTenant_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        var tenant = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jayson",
            tenantLastName: "Santiago",
            tenantEmail: "old@email.com",
            tenantPhoneNumber: "09111111111");

        var request = new
        {
            FirstName = "Jason",
            LastName = "Santiagos"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/tenants/{tenant.TenantId.Id}/personal-info",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedTenant = await dbContext.Tenants.FindAsync(tenant.TenantId);

        updatedTenant.Should().NotBeNull();
        updatedTenant!.PersonalInfo.FirstName.Should().Be("Jason");
        updatedTenant.PersonalInfo.LastName.Should().Be("Santiagos");
    }

    [Fact]
    public async Task ChangeTenantContactInfo_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var request = new
        {
            FirstName = "",
            LastName = ""
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/tenants/{tenant.TenantId.Id}/personal-info",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
