using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Buildings;

public sealed class GetAvailableBuildingsLookupTests : IntegrationTestBase
{
    public GetAvailableBuildingsLookupTests(IntegrationTestWebAppFactory factory) :
        base(factory)
    {
    }

    [Fact]
    public async Task GetAvailableBuildingsLookup_ShouldReturnResults()
    {
        await ResetDatabaseAsync();
        await BuildingSeederHelper.SeedBuildingsAsync(Factory, 100);

        var response = await Client.GetAsync("/api/buildings/available");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetAvailableBuildingsLookup_CanBeObservedForElapsedTime()
    {
        await ResetDatabaseAsync();
        await BuildingSeederHelper.SeedBuildingsAsync(Factory, 1000);

        var stopwatch = Stopwatch.StartNew();

        var response = await Client.GetAsync("/api/buildings/available");

        stopwatch.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Observation only, not a hard assertion
        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}ms");
    }
}
