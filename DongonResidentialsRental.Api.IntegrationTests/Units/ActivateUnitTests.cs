using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Unit;
using DongonResidentialsRental.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Units;

public sealed class ActivateUnitTests : IntegrationTestBase
{
    public ActivateUnitTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ActivateUnit_ShouldReturnNoContent_AndActivateUnit()
    {
        // Arrange
        await ResetDatabaseAsync();

        var unit = await UnitSeederHelper.SeedUnitAsync(Factory, status: UnitStatus.Inactive);

        // Act
        var response = await Client.PutAsync($"/api/units/{unit.UnitId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedUnit = await dbContext.Units
            .FindAsync(unit.UnitId);

        updatedUnit.Should().NotBeNull();
        updatedUnit.Status.Should().Be(UnitStatus.Active);
    }

    [Fact]
    public async Task ActivateUnit_ShouldReturnNotFound_WhenUnitDoesNotExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentUnitId = Guid.NewGuid();
        // Act
        var response = await Client.PutAsync($"/api/units/{nonExistentUnitId}/activate", null);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
