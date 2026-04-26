using AwesomeAssertions;
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

public class AddInvoiceLineTests : IntegrationTestBase
{
    public AddInvoiceLineTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddInvoiceLine_Should_Add_Line_To_Invoice_When_Request_Is_Valid()
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
           new DateOnly(2026, 1, 1),
           new DateOnly(2027, 3, 31),
           status: LeaseStatus.Active);

        var billingPeriod = BillingPeriod.Create(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var rentLine = (Description: "Rent", Quantity: 1, Amount: 1000m, LineType: InvoiceLineType.Rent);


        var lineItems = new List<(string Description, int Quantity, decimal Amount, InvoiceLineType LineType)>
        {
            rentLine
        };

        var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            billingPeriod.From,
            billingPeriod.To,
            dueDate: dueDate,
            lineItems: lineItems);


        var newLine = (Description: "Electricity", Quantity: 1, Amount: 150m, LineType: InvoiceLineType.Electricity);

        var request = new AddInvoiceLineRequest(
            Description: newLine.Description,
            Quantity: newLine.Quantity,
            Price: newLine.Amount,
            LineType: newLine.LineType);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/invoices/{invoice.InvoiceId}/lines", 
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify persisted state (use scope since DbContext is not exposed directly)
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedInvoice = await dbContext.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        updatedInvoice.Should().NotBeNull();
        updatedInvoice.Lines.Count.Should().Be(2);

        var electricityLine = updatedInvoice.Lines.SingleOrDefault(i => i.Type == InvoiceLineType.Electricity);
        electricityLine.Should().NotBeNull();
        electricityLine.Description.Should().Be(newLine.Description);
        electricityLine.Quantity.Should().Be(newLine.Quantity);
        electricityLine.UnitPrice.Amount.Should().Be(newLine.Amount);
    }

    [Fact]
    public async Task AddInvoiceLine_Should_Return_NotFound_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var invoiceId = new InvoiceId(Guid.NewGuid());

        var request = new AddInvoiceLineRequest(
            Description: "Electricity",
            Quantity: 1,
            Price: 150m,
            LineType: InvoiceLineType.Electricity);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/invoices/{invoiceId}/lines",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddInvoiceLine_Should_Return_BadRequest_When_Invoice_Is_Not_Draft()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);
        var unit = await UnitSeederHelper.SeedUnitAsync(Factory, building.BuildingId);
        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 3, 31),
            status: LeaseStatus.Active);

        var billingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

        var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            billingPeriod.From,
            billingPeriod.To,
            dueDate: dueDate,
            lineItems:
            [
                ("Rent", 1, 1000m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        var request = new AddInvoiceLineRequest(
            Description: "Electricity",
            Quantity: 1,
            Price: 150m,
            LineType: InvoiceLineType.Electricity);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/invoices/{invoice.InvoiceId}/lines",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedInvoice = await dbContext.Invoices
            .Include(i => i.Lines)
            .FirstAsync(i => i.InvoiceId == invoice.InvoiceId);

        updatedInvoice.Lines.Count.Should().Be(1);
        updatedInvoice.Lines.Should().NotContain(i => i.Type == InvoiceLineType.Electricity);
    }

    [Fact]
    public async Task AddInvoiceLine_Should_Increase_Quantity_When_Same_Line_Already_Exists()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(Factory, building.BuildingId);

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 3, 31),
            status: LeaseStatus.Active);

        var billingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

        var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);

        var existingLine = (
            Description: "Electricity",
            Quantity: 1,
            Amount: 150m,
            LineType: InvoiceLineType.Electricity);

        var invoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            billingPeriod.From,
            billingPeriod.To,
            dueDate: dueDate,
            lineItems:
            [
                existingLine
            ]);

        var request = new AddInvoiceLineRequest(
            Description: existingLine.Description,
            Quantity: 2,
            Price: existingLine.Amount,
            LineType: existingLine.LineType);

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/api/invoices/{invoice.InvoiceId}/lines",
            request,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var updatedInvoice = await dbContext.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        updatedInvoice.Should().NotBeNull();
        updatedInvoice!.Lines.Count.Should().Be(1);

        var electricityLine = updatedInvoice.Lines.Single(i => i.Type == InvoiceLineType.Electricity);

        electricityLine.Quantity.Should().Be(3);
        electricityLine.Description.Should().Be(existingLine.Description);
        electricityLine.UnitPrice.Amount.Should().Be(existingLine.Amount);
    }
}
