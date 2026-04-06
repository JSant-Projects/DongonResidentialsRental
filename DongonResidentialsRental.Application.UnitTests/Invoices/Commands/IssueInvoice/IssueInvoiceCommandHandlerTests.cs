using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;
using DongonResidentialsRental.Application.Invoices.Policies;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IInvoiceIssuancePolicy _invoiceIssuancePolicy = Substitute.For<IInvoiceIssuancePolicy>();

    private readonly IssueInvoiceCommandHandler _handler;

    public IssueInvoiceCommandHandlerTests()
    {
        _handler = new IssueInvoiceCommandHandler(
            _invoiceRepository,
            _dateTimeProvider,
            _leaseRepository, 
            _invoiceIssuancePolicy);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var invoiceId = NewInvoiceId();
        var command = new IssueInvoiceCommand(invoiceId);

        _invoiceRepository
            .GetByIdAsync(invoiceId)
            .Returns((Invoice?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{invoiceId}*");
    }

    [Fact]
    public async Task Handle_Should_Call_Issue_On_Invoice_With_Today_From_DateTimeProvider()
    {
        // Arrange
        var invoice = CreateDraftInvoice(
            new DateOnly(2026, 2, 26),
            new DateOnly(2026, 3, 26),
            new DateOnly(2026, 4, 6));

        var command = new IssueInvoiceCommand(invoice.InvoiceId);

        var lease = Lease.Create(
          occupancy: new TenantId(Guid.NewGuid()),
          unitId: new UnitId(Guid.NewGuid()),
          leaseTerm: LeaseTerm.Create(new DateOnly(2026, 2, 1), null),
          monthlyRate: Money.Create("CAD", 1200m),
          billingSettings: BillingSettings.Create(1, 5),
          utilityResponsibility: UtilityResponsibility.Create(
              tenantPaysElectricity: true,
              tenantPaysWater: false));

        var today = new DateTime(2026, 3, 27);
        var expectedDateOnly = DateOnly.FromDateTime(today);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _invoiceRepository
            .GetByIdAsync(
                invoice.InvoiceId, 
                Arg.Any<CancellationToken>())
            .Returns(invoice);

        _leaseRepository
            .GetByIdAsync(invoice.LeaseId)
            .Returns(lease);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        invoice.IssuedOn.Should().Be(expectedDateOnly);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var invoice = CreateDraftInvoice(
            new DateOnly(2026, 2, 26),
            new DateOnly(2026, 3, 26),
            new DateOnly(2026, 4, 6));

        var command = new IssueInvoiceCommand(invoice.InvoiceId);


        var lease = Lease.Create(
          occupancy: new TenantId(Guid.NewGuid()),
          unitId: new UnitId(Guid.NewGuid()),
          leaseTerm: LeaseTerm.Create(new DateOnly(2026, 2, 1), null),
          monthlyRate: Money.Create("CAD", 1200m),
          billingSettings: BillingSettings.Create(1, 5),
          utilityResponsibility: UtilityResponsibility.Create(
              tenantPaysElectricity: true,
              tenantPaysWater: false));

        _dateTimeProvider.Today.Returns(new DateOnly(2026, 3, 27));

        _invoiceRepository
            .GetByIdAsync(
                invoice.InvoiceId,
                Arg.Any<CancellationToken>())
            .Returns(invoice);

        _leaseRepository
            .GetByIdAsync(invoice.LeaseId)
            .Returns(lease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_Should_Throw_DomainException_When_InvoiceIssuancePolicy_Does_Not_Allow_Issuance()
    {
        // Arrange
        var invoice = CreateDraftInvoice(
            new DateOnly(2026, 2, 26),
            new DateOnly(2026, 3, 26),
            new DateOnly(2026, 4, 6));


        var command = new IssueInvoiceCommand(invoice.InvoiceId);

        var lease = Lease.Create(
            occupancy: new TenantId(Guid.NewGuid()),
            unitId: new UnitId(Guid.NewGuid()),
            leaseTerm: LeaseTerm.Create(new DateOnly(2026, 2, 1), null),
            monthlyRate: Money.Create("CAD", 1200m),
            billingSettings: BillingSettings.Create(1, 5),
            utilityResponsibility: UtilityResponsibility.Create(
                tenantPaysElectricity: true,
                tenantPaysWater: false));

        _invoiceRepository
            .GetByIdAsync(
                invoice.InvoiceId,
                Arg.Any<CancellationToken>())
            .Returns(invoice);

        _leaseRepository
            .GetByIdAsync(invoice.LeaseId)
            .Returns(lease);

        _invoiceIssuancePolicy
            .When(x => x.EnsureCanIssue(invoice, lease))
            .Do(_ => throw new DomainException("Electricity line is required before issuing this invoice."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("Electricity line is required before issuing this invoice.");

        _invoiceIssuancePolicy.Received(1).EnsureCanIssue(invoice, lease);
    }

    private static InvoiceId NewInvoiceId() => new InvoiceId(Guid.NewGuid());
    private static LeaseId NewLeaaseId() => new LeaseId(Guid.NewGuid());

    private static Invoice CreateDraftInvoice(
        DateOnly from,
        DateOnly to,
        DateOnly dueDate,
        decimal rentAmount = 1200m,
        string currency = "CAD")
    {
        var invoice = Invoice.Create(
            invoiceNumber: "INV-0001",
            leaseId: NewLeaaseId(),
            billingPeriod: BillingPeriod.Create(from, to),
            dueDate: dueDate,
            currency: currency);

        invoice.AddLine(
            "Monthly Rent",
            1,
            Money.Create(currency, rentAmount),
            InvoiceLineType.Rent);

        return invoice;
    }

}
