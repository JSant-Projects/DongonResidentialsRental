using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly IssueInvoiceCommandHandler _handler;

    public IssueInvoiceCommandHandlerTests()
    {
        _handler = new IssueInvoiceCommandHandler(
            _invoiceRepository,
            _dateTimeProvider);
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

        var invoiceId = NewInvoiceId();
        var command = new IssueInvoiceCommand(invoiceId);

        var today = new DateTime(2026, 3, 27);
        var expectedDateOnly = DateOnly.FromDateTime(today);

        _dateTimeProvider.Today.Returns(today);

        _invoiceRepository
            .GetByIdAsync(invoiceId)
            .Returns(invoice);

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

        var invoiceId = NewInvoiceId();
        var command = new IssueInvoiceCommand(invoiceId);

        _dateTimeProvider.Today.Returns(new DateTime(2026, 3, 27));

        _invoiceRepository
            .GetByIdAsync(invoiceId)
            .Returns(invoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
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
