using Bogus;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class BuildingSeedHelper
{
    public static async Task SeedBuildingsAsync(
        IntegrationTestWebAppFactory factory,
        int count)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var faker = new Faker();

        var buildings = Enumerable.Range(1, count)
            .Select(i =>
            {
                var building = Building.Create(
                    name: $"Building {i} {faker.Company.CompanyName()}",
                    address: Address.Create(
                        street: faker.Address.StreetAddress(),
                        city: "Camrose",
                        province: "AB",
                        postalCode: "T4V 1A1"));

                if (i % 3 == 0)
                {
                    building.Archive(); // only if your domain supports this
                }

                return building;
            })
            .ToList();

        await dbContext.Buildings.AddRangeAsync(buildings);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Building> SeedBuildingAsync(
        IntegrationTestWebAppFactory factory,
        string name = "Test Building",
        string street = "123 Main St",
        string city = "Camrose",
        string province = "AB",
        string postalCode = "T4V 1A1")
    {
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var building = Building.Create(
            name,
            Address.Create(street, city, province, postalCode));

        await dbContext.Buildings.AddAsync(building);
        await dbContext.SaveChangesAsync();

        return building;
    }
}
