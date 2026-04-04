using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Units;

public sealed class CreateUnitTests : IntegrationTestBase
{
    public CreateUnitTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUnit_ShouldReturnCreated_WhenRequestIsValid()
    {
        await ResetDatabaseAsync();
        var building = await BuildingSeedHelper.SeedBuildingAsync(Factory);
        var request = new
        {
            BuildingId = building.BuildingId.Id,
            UnitNumber = "101",
            Floor = 2
        };
        var response = await Client.PostAsJsonAsync("/api/units", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateUnit_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        await ResetDatabaseAsync();

        var request = new
        {
            BuildingId = Guid.Empty,
            UnitNumber = "",
            Floor = 2
        };

        var response = await Client.PostAsJsonAsync("/api/units", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
