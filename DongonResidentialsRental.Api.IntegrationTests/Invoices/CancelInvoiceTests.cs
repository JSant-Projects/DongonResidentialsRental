using AwesomeAssertions;
using Docker.DotNet.Models;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Invoices;

public sealed class CancelInvoiceTests : IntegrationTestBase
{
    public CancelInvoiceTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CancelInvoice_Should_Return_NoContent_When_Request_Is_Valid()
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
        var response = await Client.PostAsync(
            $"/api/invoices/{invoice.InvoiceId.Id}/cancel",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cancelledInvoice = await dbContext.Invoices
            .SingleOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        cancelledInvoice.Should().NotBeNull();
        cancelledInvoice!.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public async Task CancelInvoice_Should_Return_NotFound_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var invoiceId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync(
            $"/api/invoices/{invoiceId}/cancel",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelInvoice_Should_Return_Conflict_When_Invoice_Is_Paid()
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

        var payment = await PaymentSeederHelper.SeedPaymentAsync(
            Factory,
            tenant.TenantId,
            amount: 1500m,
            receivedOn: new DateOnly(2026, 5, 1));

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var persistedInvoice = await dbContext.Invoices
                        .Include(i => i.Lines)
                        .Include(i => i.Allocations)
                        .SingleAsync(i => i.InvoiceId == invoice.InvoiceId);

            var persistedPayment = await dbContext.Payments
                .SingleAsync(p => p.PaymentId == payment.PaymentId);

            var appliedOn = new DateOnly(2026, 5, 1);
            var amount = Money.Create("CAD", 1500m);

            persistedPayment.ApplyToInvoice(
                persistedInvoice.InvoiceId,
                amount,
                appliedOn);

            persistedInvoice.ApplyPayment(
                persistedPayment.PaymentId,
                amount,
                appliedOn);


            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await Client.PostAsync(
            $"/api/invoices/{invoice.InvoiceId.Id}/cancel",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelInvoice_Should_Return_NoContent_When_Invoice_Is_Already_Cancelled()
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
            invoiceStatus: InvoiceStatus.Cancelled);

        // Act
        var response = await Client.PostAsync(
            $"/api/invoices/{invoice.InvoiceId.Id}/cancel",
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cancelledInvoice = await dbContext.Invoices
            .SingleOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        cancelledInvoice.Should().NotBeNull();
        cancelledInvoice!.Status.Should().Be(InvoiceStatus.Cancelled);
    }
}
