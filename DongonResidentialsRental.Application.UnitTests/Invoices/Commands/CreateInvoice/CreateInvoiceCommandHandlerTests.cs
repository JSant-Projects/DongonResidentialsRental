using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;
using DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = Substitute.For<IInvoiceNumberGenerator>();

    private readonly CreateInvoiceCommandHandler _handler;
    public CreateInvoiceCommandHandlerTests()
    {
        _handler = new CreateInvoiceCommandHandler(
            _invoiceRepository,
            _leaseRepository,
            _invoiceNumberGenerator);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLease();

        var period = new DateRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var command = new CreateInvoiceCommand(
            leaseId,
            period);

        _leaseRepository
            .GetByIdAsync(leaseId, Arg.Any<CancellationToken>())
            .Returns((Lease?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{leaseId}*");

        await _invoiceRepository.DidNotReceive()
            .ExistsIssuedAsync(
                Arg.Any<LeaseId>(),
                Arg.Any<BillingPeriod>(),
                Arg.Any<CancellationToken>());

        await _invoiceNumberGenerator.DidNotReceive()
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.DidNotReceive()
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperationException_When_Issued_Invoice_Already_Exists()
    {
        // Arrange
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null,
            monthlyRentAmount: 1200m,
            currency: "CAD",
            dueDayOfMonth: 5);

        var period = new DateRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        var command = new CreateInvoiceCommand(
            lease.LeaseId,
            period);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Any<BillingPeriod>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage($"Lease {lease.LeaseId} already have an issued invoice for the current billing period.");

        await _invoiceNumberGenerator.DidNotReceive()
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.DidNotReceive()
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Create_Invoice_And_Return_InvoiceId_When_Request_Is_Valid()
    {
        // Arrange
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null,
            monthlyRentAmount: 1500m,
            currency: "CAD",
            dueDayOfMonth: 5);

        var period = new DateRange(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var command = new CreateInvoiceCommand(
            lease.LeaseId,
            period);

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Any<BillingPeriod>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202603-0001");

        Invoice? addedInvoice = null;

        _invoiceRepository
            .When(x => x.Add(Arg.Any<Invoice>()))
            .Do(callInfo => addedInvoice = callInfo.Arg<Invoice>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedInvoice.Should().NotBeNull();
        addedInvoice!.InvoiceId.Should().Be(result);
        addedInvoice.LeaseId.Should().Be(lease.LeaseId);
        addedInvoice.InvoiceNumber.Should().Be("INV-202603-0001");
        addedInvoice.BillingPeriod.From.Should().Be(new DateOnly(2026, 3, 1));
        addedInvoice.BillingPeriod.To.Should().Be(new DateOnly(2026, 3, 31));
        addedInvoice.DueDate.Should().Be(new DateOnly(2026, 3, 5));
        addedInvoice.Currency.Should().Be("CAD");

        _invoiceRepository.Received(1).Add(Arg.Any<Invoice>());
        await _invoiceNumberGenerator.Received(1).GenerateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Check_ExistsIssuedAsync_Using_Command_BillingPeriod()
    {
        // Arrange
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        var period = new DateRange(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        var command = new CreateInvoiceCommand(
            lease.LeaseId,
            period);

        BillingPeriod? capturedBillingPeriod = null;

        _leaseRepository
            .GetByIdAsync(lease.LeaseId, Arg.Any<CancellationToken>())
            .Returns(lease);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Do<BillingPeriod>(x => capturedBillingPeriod = x),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202602-0001");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedBillingPeriod.Should().NotBeNull();
        capturedBillingPeriod!.From.Should().Be(new DateOnly(2026, 2, 1));
        capturedBillingPeriod.To.Should().Be(new DateOnly(2026, 2, 28));
    }

    private static TenantId NewTenant() => new TenantId(Guid.NewGuid());
    private static UnitId NewUnit() => new UnitId(Guid.NewGuid());
    private static LeaseId NewLease() => new LeaseId(Guid.NewGuid());


    private static Lease CreateLease(
        DateOnly startDate,
        DateOnly? endDate,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        // Adapt this helper to your real Lease factory signature.
        return Lease.Create(
            occupancy: NewTenant(),
            unitId: NewUnit(),
            monthlyRate: Money.Create(currency, monthlyRentAmount),
            leaseTerm: LeaseTerm.Create(startDate, endDate),
            billingSettings: BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            utilityResponsibility: UtilityResponsibility.Create(
                tenantPaysElectricity: false,
                tenantPaysWater: false));
    }
}
