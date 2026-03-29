using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.CreditNotes.Commands.VoidCreditNote;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.CreditNotes.Commands.VoidCreditNote;

public sealed class VoidCreditNoteCommandHandlerTests
{
    private readonly ICreditNoteRepository _creditNoteRepository = Substitute.For<ICreditNoteRepository>();

    private readonly VoidCreditNoteCommandHandler _handler;

    public VoidCreditNoteCommandHandlerTests()
    {
        _handler = new VoidCreditNoteCommandHandler(_creditNoteRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_CreditNote_Does_Not_Exist()
    {
        // Arrange
        var creditNoteId = NewCreditNoteId();

        var command = new VoidCreditNoteCommand(creditNoteId);

        _creditNoteRepository
            .GetByIdAsync(creditNoteId)
            .Returns((CreditNote?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{creditNoteId}*");
    }

    [Fact]
    public async Task Handle_Should_Void_CreditNote_When_CreditNote_Exists()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var command = new VoidCreditNoteCommand(creditNote.CreditNoteId);

        _creditNoteRepository
            .GetByIdAsync(creditNote.CreditNoteId)
            .Returns(creditNote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        creditNote.Status.Should().Be(CreditNoteStatus.Voided);
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote(amount: 300m, currency: "CAD");

        var command = new VoidCreditNoteCommand(creditNote.CreditNoteId);

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

    private static CreditNote CreateIssuedCreditNote(decimal amount, string currency)
    {
        var creditNote = CreditNote.Create(
            leaseId: NewLeaseId(),
            amount: Money.Create(currency, amount));

        creditNote.Issue(new DateOnly(2026, 03, 26));

        return creditNote;
    }
}