using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Buildings;

public sealed class CreateBuildingTests : IntegrationTestBase
{
    public CreateBuildingTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateBuilding_ShouldReturnCreated_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            Name = "Pine Heights",
            AddressStreet = "123 Main St",
            AddressCity = "Camrose",
            AddressProvince = "AB",
            AddressPostalCode = "T4V 1A1"
        };

        var response = await Client.PostAsJsonAsync("/api/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateBuilding_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            Name = "",
            AddressStreet = "",
            AddressCity = "Camrose",
            AddressProvince = "AB",
            AddressPostalCode = "T4V 1A1"
        };

        var response = await Client.PostAsJsonAsync("/api/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}