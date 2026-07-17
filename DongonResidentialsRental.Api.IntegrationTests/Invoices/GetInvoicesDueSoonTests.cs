using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Invoices.Queries;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Invoices;

public sealed class GetInvoicesDueSoonTests : IntegrationTestBase
{
    public GetInvoicesDueSoonTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetInvoicesDueSoon_ReturnsInvoicesDueWithinSpecifiedDays()
    {
        // Arrange
        await ResetDatabaseAsync();

        Factory.Today = new DateOnly(2026, 5, 5);

        var daysCount = 5;

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId);

        var unit2 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var unit3 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "103");

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var tenant2 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@example.com",
            tenantPhoneNumber: "09123456788");

        var tenant3 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Alice",
            tenantLastName: "Johnson",
            tenantEmail: "alice.johnson@example.com",
            tenantPhoneNumber: "09187654321");

        var lease = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant.TenantId,
            unit.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 1500m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: false);

        var lease2 = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant2.TenantId,
            unit2.UnitId,
            startDate: new DateOnly(2026, 2, 1),
            monthlyRate: 2000m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: true);

        var lease3 = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant3.TenantId,
            unit3.UnitId,
            startDate: new DateOnly(2026, 2, 1),
            monthlyRate: 2500m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: true);

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

        var invoice2 = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease2.LeaseId,
            invoiceNumber: "INV-202603-0002",
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 2000m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        var invoice3 = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease3.LeaseId,
            invoiceNumber: "INV-202603-0003",
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 25),
            lineItems:
            [
                ("Monthly Rent", 1, 2500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        // Act

        var response = await Client.GetAsync($"/api/invoices/due-soon?days={daysCount}");

        // Assert

        var result = await response.Content.ReadFromJsonAsync<PagedResult<InvoiceResponse>>(options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        result.Items.Should().OnlyContain(
            invoice => invoice.DueDate == Factory.Today.AddDays(daysCount));
    }

    [Fact]
    public async Task GetInvoicesDueSoon_ReturnsOnlyInvoicesWithOutstandingBalance()
    {
        // Arrange
        await ResetDatabaseAsync();

        Factory.Today = new DateOnly(2026, 5, 5);

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit1 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "101");

        var unit2 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var unit3 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "103");

        var tenant1 = await TenantSeederHelper.SeedTenantAsync(
            Factory);

        var tenant2 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@example.com",
            tenantPhoneNumber: "09123456788");

        var tenant3 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Alice",
            tenantLastName: "Johnson",
            tenantEmail: "alice.johnson@example.com",
            tenantPhoneNumber: "09187654321");

        var lease1 = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant1.TenantId,
            unit1.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 1500m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: false);

        var lease2 = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant2.TenantId,
            unit2.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 2000m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: true);

        var lease3 = await LeaseSeederHelper.SeedLeaseAsync(
            Factory,
            tenant3.TenantId,
            unit3.UnitId,
            startDate: new DateOnly(2026, 1, 1),
            monthlyRate: 2500m,
            status: LeaseStatus.Active,
            tenantPaysElectricity: true,
            tenantPaysWater: true);

        var issuedInvoice = await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease1.LeaseId,
            invoiceNumber: "INV-202605-0001",
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        var partiallyPaidInvoice =
            await InvoiceSeederHelper.SeedInvoiceAsync(
                Factory,
                lease2.LeaseId,
                invoiceNumber: "INV-202605-0002",
                from: new DateOnly(2026, 4, 1),
                to: new DateOnly(2026, 4, 30),
                dueDate: new DateOnly(2026, 5, 10),
                lineItems:
                [
                    ("Monthly Rent", 1, 2000m, InvoiceLineType.Rent)
                ],
                invoiceStatus: InvoiceStatus.Issued);

        var fullyPaidInvoice =
            await InvoiceSeederHelper.SeedInvoiceAsync(
                Factory,
                lease3.LeaseId,
                invoiceNumber: "INV-202605-0003",
                from: new DateOnly(2026, 4, 1),
                to: new DateOnly(2026, 4, 30),
                dueDate: new DateOnly(2026, 5, 10),
                lineItems:
                [
                    ("Monthly Rent", 1, 2500m, InvoiceLineType.Rent)
                ],
                invoiceStatus: InvoiceStatus.Issued);

        var partialPayment =
            await PaymentSeederHelper.SeedPaymentAsync(
                Factory,
                tenant2.TenantId,
                amount: 500m,
                receivedOn: new DateOnly(2026, 5, 1));

        var fullPayment =
            await PaymentSeederHelper.SeedPaymentAsync(
                Factory,
                tenant3.TenantId,
                amount: 2500m,
                receivedOn: new DateOnly(2026, 5, 1));

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var persistedPartiallyPaidInvoice = await dbContext.Invoices
                .Include(i => i.Lines)
                .Include(i => i.Allocations)
                .SingleAsync(
                    i => i.InvoiceId == partiallyPaidInvoice.InvoiceId);

            var persistedFullyPaidInvoice = await dbContext.Invoices
                .Include(i => i.Lines)
                .Include(i => i.Allocations)
                .SingleAsync(
                    i => i.InvoiceId == fullyPaidInvoice.InvoiceId);

            var persistedPartialPayment = await dbContext.Payments
                .Include(p => p.Allocations)
                .SingleAsync(
                    p => p.PaymentId == partialPayment.PaymentId);

            var persistedFullPayment = await dbContext.Payments
                .Include(p => p.Allocations)
                .SingleAsync(
                    p => p.PaymentId == fullPayment.PaymentId);

            var appliedOn = new DateOnly(2026, 5, 1);

            var partialAmount = Money.Create("CAD", 500m);

            persistedPartialPayment.ApplyToInvoice(
                persistedPartiallyPaidInvoice.InvoiceId,
                partialAmount,
                appliedOn);

            persistedPartiallyPaidInvoice.ApplyPayment(
                persistedPartialPayment.PaymentId,
                partialAmount,
                appliedOn);

            var fullAmount = Money.Create("CAD", 2500m);

            persistedFullPayment.ApplyToInvoice(
                persistedFullyPaidInvoice.InvoiceId,
                fullAmount,
                appliedOn);

            persistedFullyPaidInvoice.ApplyPayment(
                persistedFullPayment.PaymentId,
                fullAmount,
                appliedOn);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync(
            "/api/invoices/due-soon?days=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<InvoiceResponse>>(
                options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        result.Items.Should().Contain(
            item =>
                item.InvoiceId == issuedInvoice.InvoiceId.Id &&
                item.Status == InvoiceStatus.Issued &&
                item.Balance == 1500m);

        result.Items.Should().Contain(
            item =>
                item.InvoiceId == partiallyPaidInvoice.InvoiceId.Id &&
                item.Balance == 1500m);

        result.Items.Should().NotContain(
            item => item.InvoiceId == fullyPaidInvoice.InvoiceId.Id);
    }
}
