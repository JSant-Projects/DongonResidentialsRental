using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.Contracts.Invoices;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Invoices;

public sealed class CreateInvoiceTests : IntegrationTestBase
{
    public CreateInvoiceTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateInvoice_Should_Return_Created_When_Request_Is_Valid()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 1500m,
            status: LeaseStatus.Active);

        var request = new CreateInvoiceRequest(
            lease.LeaseId.Id,
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30));

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/invoices",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var invoiceId = await response.Content.ReadFromJsonAsync<Guid>();
        invoiceId.Should().NotBeEmpty();

        // Verify persisted state (use scope since DbContext is not exposed directly)
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invoice = await dbContext.Invoices
            .Include(i => i.Lines)
            .SingleOrDefaultAsync(i => i.InvoiceId == new InvoiceId(invoiceId));

        invoice.Should().NotBeNull();
        invoice!.LeaseId.Should().Be(lease.LeaseId);

        invoice.BillingPeriod.From.Should().Be(new DateOnly(2026, 4, 1));
        invoice.BillingPeriod.To.Should().Be(new DateOnly(2026, 4, 30));

        invoice.Currency.Should().Be("CAD");

        invoice.Lines.Should().ContainSingle();

        var rentLine = invoice.Lines.Single();

        rentLine.Description.Should().Be("Monthly Rent");
        rentLine.Quantity.Should().Be(1);
        rentLine.UnitPrice.Amount.Should().Be(1500m);
        rentLine.UnitPrice.Currency.Should().Be("CAD");
        rentLine.Type.Should().Be(InvoiceLineType.Rent);
    }

    [Fact]
    public async Task CreateInvoice_Should_Return_NotFound_When_Lease_Does_Not_Exist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = new CreateInvoiceRequest(
            Guid.NewGuid(),
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30));

        // Act
        var response = await Client.PostAsync(
            "/api/invoices",
            CreateJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateInvoice_Should_Return_BadRequest_When_Period_Is_Invalid()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 1500m,
            status: LeaseStatus.Active);

        var request = new CreateInvoiceRequest(
            lease.LeaseId.Id,
            new DateOnly(2026, 4, 30), // invalid
            new DateOnly(2026, 4, 1));

        // Act
        var response = await Client.PostAsync(
            "/api/invoices",
            CreateJsonContent(request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
