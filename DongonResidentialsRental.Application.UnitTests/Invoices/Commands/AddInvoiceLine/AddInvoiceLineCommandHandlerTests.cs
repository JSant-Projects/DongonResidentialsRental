using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Commands.AddInvoiceLine;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.AddInvoiceLine;

public sealed class AddInvoiceLineCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();

    private readonly AddInvoiceLineCommandHandler _handler;

    public AddInvoiceLineCommandHandlerTests()
    {
        _handler = new AddInvoiceLineCommandHandler(_invoiceRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var invoiceId = NewInvoiceId();

        var command = new AddInvoiceLineCommand(
            invoiceId,
            "Electricity",
            1,
            150m,
            InvoiceLineType.Electricity);

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
    public async Task Handle_Should_Add_Electricity_Line_To_Invoice()
    {
        // Arrange
        var invoice = CreateDraftInvoiceWithRent();

        var command = new AddInvoiceLineCommand(
            invoice.InvoiceId,
            "Electricity",
            1,
            150m,
            InvoiceLineType.Electricity);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        invoice.Lines.Should().HaveCount(2);

        var electricityLine = invoice.Lines
            .FirstOrDefault(x => x.Type == InvoiceLineType.Electricity);

        electricityLine.Should().NotBeNull();
        electricityLine!.Description.Should().Be("Electricity");
        electricityLine.Quantity.Should().Be(1);
        electricityLine.UnitPrice.Amount.Should().Be(150m);
        electricityLine.UnitPrice.Currency.Should().Be("CAD");

        // Optional but nice:
        invoice.Total.Amount.Should().Be(1350m); // 1200 rent + 150 electricity
    }

    [Fact]
    public async Task Handle_Should_Use_Invoice_Currency_When_Creating_Money()
    {
        // Arrange
        var invoice = CreateDraftInvoiceWithRent(currency: "CAD");

        var command = new AddInvoiceLineCommand(
            invoice.InvoiceId,
            "Electricity",
            1,
            150m,
            InvoiceLineType.Electricity);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var electricityLine = invoice.Lines
            .First(x => x.Type == InvoiceLineType.Electricity);

        electricityLine.UnitPrice.Currency.Should().Be(invoice.Currency);
    }


    private static InvoiceId NewInvoiceId() => new InvoiceId(Guid.NewGuid());
    private static LeaseId NewLeaaseId() => new LeaseId(Guid.NewGuid());

    private static Invoice CreateDraftInvoiceWithRent(string currency = "CAD")
    {
        var invoice = Invoice.Create(
            invoiceNumber: "INV-0001",
            leaseId: NewLeaaseId(),
            billingPeriod: BillingPeriod.Create(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31)),
            dueDate: new DateOnly(2026, 3, 5),
            currency: currency);

        invoice.AddLine(
            "Monthly Rent",
            1,
            Money.Create(currency, 1200m),
            InvoiceLineType.Rent);

        return invoice;
    }
}
