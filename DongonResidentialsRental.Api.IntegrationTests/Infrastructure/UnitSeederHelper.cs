using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class UnitSeederHelper
{
    public static async Task<Unit> SeedUnitAsync(
        IntegrationTestWebAppFactory factory,
        string buildingName = "Test Building",
        string unitNumber = "101",
        int? floor = 1)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var building = Building.Create(
            buildingName,
            Address.Create(
                "123 Main St",
                "Camrose",
                "AB",
                "T4V 1A1"));

        var unit = Unit.Create(
            building.BuildingId,
            unitNumber, 
            floor);

        await dbContext.Buildings.AddAsync(building);
        await dbContext.Units.AddAsync(unit);
        await dbContext.SaveChangesAsync();

        return unit;
    }
}
