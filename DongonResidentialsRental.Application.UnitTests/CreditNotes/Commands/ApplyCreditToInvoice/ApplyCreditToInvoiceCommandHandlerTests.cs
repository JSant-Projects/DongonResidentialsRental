using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.CreditNotes.Commands.ApplyCreditToInvoice;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.CreditNotes.Commands.ApplyCreditToInvoice;

public sealed class ApplyCreditToInvoiceCommandHandlerTests
{
    private readonly ICreditNoteRepository _creditNoteRepository = Substitute.For<ICreditNoteRepository>();
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ApplyCreditToInvoiceCommandHandler _handler;

    public ApplyCreditToInvoiceCommandHandlerTests()
    {
        _handler = new ApplyCreditToInvoiceCommandHandler(
            _creditNoteRepository,
            _invoiceRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var command = new ApplyCreditToInvoiceCommand(
            NewCreditNoteId(),
            NewInvoiceId(),
            100m);

        _invoiceRepository
            .GetByIdAsync(command.InvoiceId)
            .Returns((Invoice?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{command.InvoiceId}*");

        await _creditNoteRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<CreditNoteId>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_CreditNote_Does_Not_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");

        var command = new ApplyCreditToInvoiceCommand(
            NewCreditNoteId(),
            invoice.InvoiceId,
            100m);

        _invoiceRepository
            .GetByIdAsync(command.InvoiceId)
            .Returns(invoice);

        _creditNoteRepository
            .GetByIdAsync(command.CreditNoteId)
            .Returns((CreditNote?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{command.CreditNoteId}*");
    }

    [Fact]
    public async Task Handle_Should_Apply_Credit_To_Both_CreditNote_And_Invoice_When_Request_Is_Valid()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 10, 0, 0);
        var expectedAppliedOn = DateOnly.FromDateTime(today);

        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var command = new ApplyCreditToInvoiceCommand(
            creditNote.CreditNoteId,
            invoice.InvoiceId,
            100m);

        _dateTimeProvider.Today.Returns(today);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        _creditNoteRepository
            .GetByIdAsync(creditNote.CreditNoteId)
            .Returns(creditNote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        creditNote.Allocations.Should().HaveCount(1);
        creditNote.Allocations.Single().InvoiceId.Should().Be(invoice.InvoiceId);
        creditNote.Allocations.Single().Amount.Amount.Should().Be(100m);
        creditNote.Allocations.Single().Amount.Currency.Should().Be("CAD");
        creditNote.Allocations.Single().AppliedOn.Should().Be(expectedAppliedOn);

        invoice.CreditAllocations.Should().HaveCount(1);
        invoice.CreditAllocations.Single().CreditNoteId.Should().Be(creditNote.CreditNoteId);
        invoice.CreditAllocations.Single().Amount.Amount.Should().Be(100m);
        invoice.CreditAllocations.Single().Amount.Currency.Should().Be("CAD");
        invoice.CreditAllocations.Single().AppliedOn.Should().Be(expectedAppliedOn);

        creditNote.RemainingAmount.Amount.Should().Be(200m);
        invoice.Balance.Amount.Should().Be(1100m);
    }

    [Fact]
    public async Task Handle_Should_Use_Today_From_DateTimeProvider_When_Applying_Credit()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 15, 30, 0);
        var expectedAppliedOn = DateOnly.FromDateTime(today);

        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var command = new ApplyCreditToInvoiceCommand(
            creditNote.CreditNoteId,
            invoice.InvoiceId,
            150m);

        _dateTimeProvider.Today.Returns(today);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        _creditNoteRepository
            .GetByIdAsync(creditNote.CreditNoteId)
            .Returns(creditNote);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        creditNote.Allocations.Single().AppliedOn.Should().Be(expectedAppliedOn);
        invoice.CreditAllocations.Single().AppliedOn.Should().Be(expectedAppliedOn);
    }

    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static CreditNoteId NewCreditNoteId() => new CreditNoteId(Guid.NewGuid());
    private static InvoiceId NewInvoiceId() => new InvoiceId(Guid.NewGuid());
    private static Invoice CreateIssuedInvoice(decimal totalAmount, string currency)
    {
        var invoice = Invoice.Create(
            invoiceNumber: "INV-0001",
            leaseId: NewLeaseId(),
            billingPeriod: BillingPeriod.Create(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31)),
            dueDate: new DateOnly(2026, 3, 5),
            currency: currency);

        invoice.AddLine(
            "Monthly Rent",
            1,
            Money.Create(currency, totalAmount),
            InvoiceLineType.Rent);

        invoice.Issue(new DateOnly(2026, 3, 1));

        return invoice;
    }

    private static CreditNote CreateIssuedCreditNote(decimal amount, string currency)
    {
        var creditNote = CreditNote.Create(
            leaseId: NewLeaseId(),
            amount: Money.Create(currency, amount));

        creditNote.Issue(new DateOnly(2026, 03, 26));

        return creditNote;
    }
}