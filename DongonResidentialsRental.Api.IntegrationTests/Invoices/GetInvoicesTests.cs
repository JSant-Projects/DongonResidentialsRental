using AwesomeAssertions;
using DongonResidentialsRental.Api.IntegrationTests.Infrastructure;
using DongonResidentialsRental.Application.Invoices.Queries;
using DongonResidentialsRental.Application.Leases.Queries;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Invoices;

public sealed class GetInvoicesTests : IntegrationTestBase
{
    public GetInvoicesTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }
    [Fact]
    public async Task GetInvoices_ShouldReturnOk_WhenInvoicesExist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId);

        var unit2 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var tenant2 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@example.com", 
            tenantPhoneNumber: "09123456788");

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

        // Act
        var response = await Client.GetAsync("/api/invoices");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<InvoiceResponse>>(options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetInvoices_ShouldFilterByLeaseId()
    {
        // Arrange
        await ResetDatabaseAsync();

        var building = await BuildingSeederHelper.SeedBuildingAsync(Factory);

        var unit = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId);

        var unit2 = await UnitSeederHelper.SeedUnitAsync(
            Factory,
            building.BuildingId,
            unitNumber: "102");

        var tenant = await TenantSeederHelper.SeedTenantAsync(Factory);

        var tenant2 = await TenantSeederHelper.SeedTenantAsync(
            Factory,
            tenantFirstName: "Jane",
            tenantLastName: "Smith",
            tenantEmail: "jane.smith@example.com",
            tenantPhoneNumber: "09123456788");

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

        // Act
        var response = await Client.GetAsync($"/api/invoices?leaseId={lease.LeaseId}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<InvoiceResponse>>(options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result!.Items[0].LeaseId.Should().Be(lease.LeaseId.Id);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetInvoices_ShouldReturnPagedResult_WhenPaginationIsApplied()
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
            tenantPaysElectricity: true,
            tenantPaysWater: false);

        await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            invoiceNumber: "INV-202604-0001",
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            invoiceNumber: "INV-202605-0002",
            from: new DateOnly(2026, 5, 1),
            to: new DateOnly(2026, 5, 31),
            dueDate: new DateOnly(2026, 6, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            invoiceNumber: "INV-202606-0003",
            from: new DateOnly(2026, 6, 1),
            to: new DateOnly(2026, 6, 30),
            dueDate: new DateOnly(2026, 7, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        // Act
        var response = await Client.GetAsync("/api/invoices?page=2&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<InvoiceResponse>>(options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetInvoices_ShouldFilterByPeriod()
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
            tenantPaysElectricity: true,
            tenantPaysWater: false);

        await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            invoiceNumber: "INV-202604-0001",
            from: new DateOnly(2026, 4, 1),
            to: new DateOnly(2026, 4, 30),
            dueDate: new DateOnly(2026, 5, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        await InvoiceSeederHelper.SeedInvoiceAsync(
            Factory,
            lease.LeaseId,
            invoiceNumber: "INV-202605-0002",
            from: new DateOnly(2026, 5, 1),
            to: new DateOnly(2026, 5, 31),
            dueDate: new DateOnly(2026, 6, 10),
            lineItems:
            [
                ("Monthly Rent", 1, 1500m, InvoiceLineType.Rent)
            ],
            invoiceStatus: InvoiceStatus.Issued);

        // Act
        var response = await Client.GetAsync(
            "/api/invoices?from=2026-05-01&to=2026-05-31");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<InvoiceResponse>>(options: JsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].From.Should().Be(new DateOnly(2026, 5, 1));
        result.Items[0].To.Should().Be(new DateOnly(2026, 5, 31));
        result.TotalCount.Should().Be(1);
    }
}
