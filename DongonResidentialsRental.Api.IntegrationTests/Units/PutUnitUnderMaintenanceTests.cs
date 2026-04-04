using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Units;

public sealed class PutUnitUnderMaintenanceTests : IntegrationTestBase
{
    public PutUnitUnderMaintenanceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PutUnitUnderMaintenance_ShouldReturnNoContent_AndPutUnitUnderMaintenance()
    {
        // Arrange
        await ResetDatabaseAsync();
        var unit = await UnitSeederHelper.SeedUnitAsync(Factory);

        // Act
        var response = await Client.PutAsync($"/api/units/{unit.UnitId}/maintenance", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedUnit = await dbContext.Units
            .FindAsync(unit.UnitId);

        updatedUnit.Should().NotBeNull();
        updatedUnit.Status.Should().Be(UnitStatus.Maintenance);
    }

    [Fact]
    public async Task PutUnderMaintenance_ShouldReturnNotFound_WhenUnitDoesNotExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentUnitId = Guid.NewGuid();
        // Act
        var response = await Client.PutAsync($"/api/units/{nonExistentUnitId}/maintenance", null);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
