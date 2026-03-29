using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.CreditNotes.Commands.RemoveCreditFromInvoice;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.CreditNotes.Commands.RemoveCreditFromInvoice;

public sealed class RemoveCreditFromInvoiceCommandHandlerTests
{
    private readonly ICreditNoteRepository _creditNoteRepository = Substitute.For<ICreditNoteRepository>();
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();

    private readonly RemoveCreditFromInvoiceCommandHandler _handler;

    public RemoveCreditFromInvoiceCommandHandlerTests()
    {
        _handler = new RemoveCreditFromInvoiceCommandHandler(
            _creditNoteRepository,
            _invoiceRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var command = new RemoveCreditFromInvoiceCommand(
            NewCreditNoteId(),
            NewInvoiceId());

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

        var command = new RemoveCreditFromInvoiceCommand(
            NewCreditNoteId(),
            invoice.InvoiceId);

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
    public async Task Handle_Should_Remove_Credit_Allocation_From_Both_CreditNote_And_Invoice_When_Request_Is_Valid()
    {
        // Arrange
        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var appliedAmount = Money.Create("CAD", 100m);
        var appliedOn = new DateOnly(2026, 3, 20);

        creditNote.ApplyToInvoice(invoice.InvoiceId, appliedAmount, appliedOn);
        invoice.ApplyCredit(creditNote.CreditNoteId, appliedAmount, appliedOn);

        var command = new RemoveCreditFromInvoiceCommand(
            creditNote.CreditNoteId,
            invoice.InvoiceId);

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

        creditNote.Allocations.Should().BeEmpty();
        invoice.CreditAllocations.Should().BeEmpty();

        creditNote.RemainingAmount.Amount.Should().Be(300m);
        invoice.Balance.Amount.Should().Be(1200m);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Request_Is_Valid()
    {
        // Arrange
        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var appliedAmount = Money.Create("CAD", 100m);
        var appliedOn = new DateOnly(2026, 3, 20);

        creditNote.ApplyToInvoice(invoice.InvoiceId, appliedAmount, appliedOn);
        invoice.ApplyCredit(creditNote.CreditNoteId, appliedAmount, appliedOn);

        var command = new RemoveCreditFromInvoiceCommand(
            creditNote.CreditNoteId,
            invoice.InvoiceId);

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
