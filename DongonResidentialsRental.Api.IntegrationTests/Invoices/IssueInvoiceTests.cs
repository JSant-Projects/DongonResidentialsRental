using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Invoices;

public sealed class IssueInvoiceTests : IntegrationTestBase
{
    public IssueInvoiceTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task IssueInvoice_Should_Return_NoContent_When_Request_Is_Valid()
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
            status: LeaseStatus.Active,
            tenantPaysElectricity: false,
            tenantPaysWater: false);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ]);

        // Act
        var response = await Client.PutAsync(
            $"/api/invoices/{invoice.InvoiceId}/issue",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var issuedInvoice = await dbContext.Invoices
            .SingleOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        issuedInvoice.Should().NotBeNull();
        issuedInvoice!.Status.Should().Be(InvoiceStatus.Issued);
        issuedInvoice.IssuedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task IssueInvoice_Should_Return_NotFound_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var invoiceId = new InvoiceId(Guid.NewGuid());

        // Act
        var response = await Client.PutAsync(
            $"/api/invoices/{invoiceId}/issue",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task IssueInvoice_Should_Return_Conflict_When_Invoice_Is_Already_Issued()
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
            status: LeaseStatus.Active,
            tenantPaysElectricity: false,
            tenantPaysWater: false);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        // Act
        var response = await Client.PutAsync(
            $"/api/invoices/{invoice.InvoiceId}/issue",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task IssueInvoice_Should_Return_Conflict_When_Lease_Is_Not_Active()
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
            status: LeaseStatus.Draft,
            tenantPaysElectricity: false,
            tenantPaysWater: false);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ]);

        // Act
        var response = await Client.PutAsync(
            $"/api/invoices/{invoice.InvoiceId}/issue",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
