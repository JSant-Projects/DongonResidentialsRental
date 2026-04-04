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
    public static async Task SeedUnitsAsync(
        IntegrationTestWebAppFactory factory,
        params Unit[] units)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Units.AddRangeAsync(units);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Unit> SeedUnitAsync(
        IntegrationTestWebAppFactory factory,
        BuildingId buildingId,
        string unitNumber = "101",
        int? floor = 1,
        UnitStatus status = UnitStatus.Active)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var unit = Unit.Create(
            buildingId,
            unitNumber,
            floor);

        switch (status)
        {
            case UnitStatus.Active:
                break;
            case UnitStatus.Inactive:
                unit.Deactivate();
                break;
            case UnitStatus.Maintenance:
                unit.PutUnderMaintenance();
                break;
        }

        await dbContext.Units.AddAsync(unit);
        await dbContext.SaveChangesAsync();

        return unit;
    }

    public static async Task<Unit> SeedUnitAsync(
        IntegrationTestWebAppFactory factory,
        string buildingName = "Test Building",
        string unitNumber = "101",
        int? floor = 1,
        UnitStatus status = UnitStatus.Active)
    {
        var building = await  BuildingSeedHelper.SeedBuildingAsync(
            factory,
            buildingName);

        return await SeedUnitAsync(
            factory,
            building.BuildingId,
            unitNumber,
            floor,
            status);
    }

    public static async Task<Unit> SeedInactiveUnitAsync(
        IntegrationTestWebAppFactory factory,
        BuildingId buildingId,
        string unitNumber = "101",
        int? floor = 1)
    {
        return await SeedUnitAsync(
            factory,
            buildingId,
            unitNumber,
            floor,
            UnitStatus.Inactive);
    }

    public static async Task<Unit> CreateUnitAsync(
        IntegrationTestWebAppFactory factory,
        BuildingId buildingId,
        UnitStatus status,
        string unitNumber = "101",
        int? floor = 1)
    {
        return await SeedUnitAsync(
            factory,
            buildingId,
            unitNumber,
            floor,
            status);
    }
}
