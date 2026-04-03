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

public sealed class ChangeTenantContactInfoTests : IntegrationTestBase
{
    public ChangeTenantContactInfoTests(IntegrationTestWebAppFactory factory)
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
            Email = "new@email.com",
            PhoneNumber = "09999999999"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/tenants/{tenant.TenantId.Id}/contact-info",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedTenant = await dbContext.Tenants.FindAsync(tenant.TenantId);

        updatedTenant.Should().NotBeNull();
        updatedTenant!.ContactInfo.Email.Value.Should().Be("new@email.com");
        updatedTenant.ContactInfo.PhoneNumber.Value.Should().Be("09999999999");
    }

    [Fact]
    public async Task ChangeTenantContactInfo_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var request = new
        {
            Email = "",
            PhoneNumber = ""
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/tenants/{tenant.TenantId.Id}/contact-info",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}