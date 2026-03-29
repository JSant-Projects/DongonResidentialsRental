using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Commands.CancelInvoice;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.Cancelnvoice;

public sealed class CancelInvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();

    private readonly CancelInvoiceCommandHandler _handler;

    public CancelInvoiceCommandHandlerTests()
    {
        _handler = new CancelInvoiceCommandHandler(_invoiceRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var invoiceId = NewInvoiceId();
        var command = new CancelInvoiceCommand(invoiceId);

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
    public async Task Handle_Should_Cancel_Invoice_When_Invoice_Exists()
    {
        // Arrange
        var invoice = CreateIssuedInvoice(); // important: must be cancellable state

        var command = new CancelInvoiceCommand(invoice.InvoiceId);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var invoice = CreateIssuedInvoice();

        var command = new CancelInvoiceCommand(invoice.InvoiceId);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }


    private static InvoiceId NewInvoiceId() => new InvoiceId(Guid.NewGuid());
    private static LeaseId NewLeaaseId() => new LeaseId(Guid.NewGuid());

    private static Invoice CreateIssuedInvoice()
    {
        var invoice = Invoice.Create(
            invoiceNumber: "INV-0001",
            leaseId: NewLeaaseId(),
            billingPeriod: BillingPeriod.Create(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31)),
            dueDate: new DateOnly(2026, 3, 5),
            currency: "CAD");

        invoice.AddLine(
            "Monthly Rent",
            1,
            Money.Create("CAD", 1200m),
            InvoiceLineType.Rent);

        // Move to Issued state (since Cancel likely requires it)
        invoice.Issue(new DateOnly(2026, 3, 1));

        return invoice;
    }
}
