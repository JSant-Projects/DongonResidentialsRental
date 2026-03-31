using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.CreditNotes.Commands.IssueCreditNote;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.CreditNotes.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandHandlerTests
{
    private readonly ICreditNoteRepository _creditNoteRepository = Substitute.For<ICreditNoteRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly IssueCreditNoteCommandHandler _handler;

    public IssueCreditNoteCommandHandlerTests()
    {
        _handler = new IssueCreditNoteCommandHandler(
            _creditNoteRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_CreditNote_Does_Not_Exist()
    {
        // Arrange
        var creditNoteId = NewCreditNoteId();
        var command = new IssueCreditNoteCommand(creditNoteId);

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
    public async Task Handle_Should_Issue_CreditNote_And_Set_IssuedOn_When_CreditNote_Exists()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 11, 0, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var creditNote = CreateCreditNote(amount: 300m, currency: "CAD");

        var command = new IssueCreditNoteCommand(creditNote.CreditNoteId);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _creditNoteRepository
            .GetByIdAsync(creditNote.CreditNoteId)
            .Returns(creditNote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        creditNote.Status.Should().Be(CreditNoteStatus.Issued);
        creditNote.IssuedOn.Should().Be(expectedDate);
    }

    [Fact]
    public async Task Handle_Should_Use_Today_From_DateTimeProvider_When_Issuing_CreditNote()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 18, 45, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var creditNote = CreateCreditNote(amount: 300m, currency: "CAD");

        var command = new IssueCreditNoteCommand(creditNote.CreditNoteId);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _creditNoteRepository
            .GetByIdAsync(creditNote.CreditNoteId)
            .Returns(creditNote);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        creditNote.IssuedOn.Should().Be(expectedDate);
    }

    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static CreditNoteId NewCreditNoteId() => new CreditNoteId(Guid.NewGuid());

    private static CreditNote CreateCreditNote(decimal amount, string currency)
    {
        var creditNote = CreditNote.Create(
            leaseId: NewLeaseId(),
            amount: Money.Create(currency, amount));

        //creditNote.Issue(new DateOnly(2026, 03, 26));

        return creditNote;
    }
}
