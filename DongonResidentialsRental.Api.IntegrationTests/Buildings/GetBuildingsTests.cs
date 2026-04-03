using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Buildings;

public sealed class GetBuildingsTests : IntegrationTestBase
{
    public GetBuildingsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetBuildings_ShouldReturnPagedResults()
    {
        await ResetDatabaseAsync();
        await BuildingSeedHelper.SeedBuildingsAsync(Factory, 100);

        var response = await Client.GetAsync("/api/buildings?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetBuildings_ShouldFilterByStatus()
    {
        await ResetDatabaseAsync();
        await BuildingSeedHelper.SeedBuildingsAsync(Factory, 50);

        var response = await Client.GetAsync("/api/buildings?status=Active&page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetBuildings_ShouldSupportSearchTerm()
    {
        await ResetDatabaseAsync();

        // You can seed targeted data manually
        // plus bulk fake data
        await BuildingSeedHelper.SeedBuildingsAsync(Factory, 50);

        var response = await Client.GetAsync("/api/buildings?searchTerm=Camrose&page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBuildings_CanBeObservedForElapsedTime()
    {
        await ResetDatabaseAsync();
        await BuildingSeedHelper.SeedBuildingsAsync(Factory, 1000);

        var stopwatch = Stopwatch.StartNew();

        var response = await Client.GetAsync("/api/buildings?page=1&pageSize=20");

        stopwatch.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Observation only, not a hard assertion
        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}ms");
    }
}
