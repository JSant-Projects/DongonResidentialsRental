using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Tenants;

public sealed class CreateTenantTests : IntegrationTestBase
{
    public CreateTenantTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    { 
    }

    [Fact]
    public async Task CreateTenant_ShouldReturnCreated_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
            PhoneNumber = "09123456789"
        };

        var response = await Client.PostAsJsonAsync("/api/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateTenant_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();
        var request = new
        {
            FirstName = "",
            LastName = "",
            Email = "john.doe@email.com",
            PhoneNumber = "09123456789"
        };

        var response = await Client.PostAsJsonAsync("/api/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
