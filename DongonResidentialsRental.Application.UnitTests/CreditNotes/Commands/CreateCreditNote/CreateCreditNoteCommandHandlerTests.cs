using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.CreditNotes.Commands.CreateCreditNote;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.CreditNotes.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandHandlerTests
{
    private readonly ICreditNoteRepository _creditNoteRepository = Substitute.For<ICreditNoteRepository>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();

    private readonly CreateCreditNoteCommandHandler _handler;

    public CreateCreditNoteCommandHandlerTests()
    {
        _handler = new CreateCreditNoteCommandHandler(
            _creditNoteRepository,
            _leaseRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Lease_Does_Not_Exist()
    {
        // Arrange
        var leaseId = NewLeaseId();

        var command = new CreateCreditNoteCommand(
            leaseId,
            150m,
            "CAD");

        _leaseRepository
            .GetByIdAsync(leaseId)
            .Returns((Lease?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{leaseId}*");

        _creditNoteRepository.DidNotReceive()
            .Add(Arg.Any<CreditNote>());
    }

    [Fact]
    public async Task Handle_Should_Create_CreditNote_Add_To_Repository_And_Return_CreditNoteId_When_Request_Is_Valid()
    {
        // Arrange
        var lease = CreateLease();

        var command = new CreateCreditNoteCommand(
            lease.LeaseId,
            150m,
            "CAD");

        _leaseRepository
            .GetByIdAsync(lease.LeaseId)
            .Returns(lease);

        CreditNote? addedCreditNote = null;

        _creditNoteRepository
            .When(x => x.Add(Arg.Any<CreditNote>()))
            .Do(callInfo => addedCreditNote = callInfo.Arg<CreditNote>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedCreditNote.Should().NotBeNull();
        addedCreditNote!.CreditNoteId.Should().Be(result);
        addedCreditNote.LeaseId.Should().Be(lease.LeaseId);
        addedCreditNote.Amount.Currency.Should().Be("CAD");
        addedCreditNote.Amount.Amount.Should().Be(150m);

        _creditNoteRepository.Received(1)
            .Add(Arg.Any<CreditNote>());
    }

    [Fact]
    public async Task Handle_Should_Create_CreditNote_Using_Command_Currency_And_Amount()
    {
        // Arrange
        var lease = CreateLease();

        var command = new CreateCreditNoteCommand(
            lease.LeaseId,
            275.50m,
            "USD");

        _leaseRepository
            .GetByIdAsync(lease.LeaseId)
            .Returns(lease);

        CreditNote? addedCreditNote = null;

        _creditNoteRepository
            .When(x => x.Add(Arg.Any<CreditNote>()))
            .Do(callInfo => addedCreditNote = callInfo.Arg<CreditNote>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedCreditNote.Should().NotBeNull();
        addedCreditNote!.Amount.Currency.Should().Be("USD");
        addedCreditNote.Amount.Amount.Should().Be(275.50m);
    }

    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static CreditNoteId NewCreditNoteId() => new CreditNoteId(Guid.NewGuid());
    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());
    private static UnitId NewUnitId() => new UnitId(Guid.NewGuid());

    private static Lease CreateLease(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        return Lease.Create(
            NewTenantId(),
            NewUnitId(),
            LeaseTerm.Create(startDate ?? new DateOnly(2026, 1, 1), endDate),
            Money.Create(currency, monthlyRentAmount),
            BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            UtilityResponsibility.Create(false, false));
    }
}
