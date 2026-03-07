using AwesomeAssertions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Tests.Domain.CreditNotes;

public sealed class CreditNoteSpecifications
{
    // ---------- Create ----------

    [Fact]
    public void Create_Should_Create_Draft_CreditNote_When_Arguments_Are_Valid()
    {
        // Arrange
        var leaseId = NewLeaseId();
        var amount = Money.Create("CAD", 100m);

        // Act
        var creditNote = CreditNote.Create(leaseId, amount);

        // Assert
        creditNote.CreditNoteId.Should().NotBeNull();
        creditNote.LeaseId.Should().Be(leaseId);
        creditNote.Amount.Should().Be(amount);
        creditNote.AmountApplied.Should().Be(Money.Zero("CAD"));
        creditNote.RemainingAmount.Should().Be(amount);
        creditNote.IssuedOn.Should().BeNull();
        creditNote.Status.Should().Be(CreditNoteStatus.Draft);
        creditNote.Allocations.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_Amount_Is_Zero()
    {
        // Arrange
        var leaseId = NewLeaseId();
        var amount = Money.Create("CAD", 0m);

        // Act
        Action act = () => CreditNote.Create(leaseId, amount);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Credit note amount must be greater than zero.");
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_Amount_Is_Negative()
    {
        // Arrange
        var leaseId = NewLeaseId();
        var amount = Money.Create("CAD", 10m);

        // Act
        Action act = () => CreditNote.Create(leaseId, amount.Negate());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Credit note amount must be greater than zero.*");
    }

    // ---------- Issue ----------

    [Fact]
    public void Issue_Should_Set_Status_To_Issued_And_Set_IssuedOn_When_CreditNote_Is_Draft()
    {
        // Arrange
        var creditNote = CreateDraftCreditNote("CAD", 100m);
        var issuedOn = Today();

        // Act
        creditNote.Issue(issuedOn);

        // Assert
        creditNote.Status.Should().Be(CreditNoteStatus.Issued);
        creditNote.IssuedOn.Should().Be(issuedOn);
    }

    [Fact]
    public void Issue_Should_Throw_DomainException_When_CreditNote_Is_Not_Draft()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        Action act = () => creditNote.Issue(Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation allowed only when credit note is in Draft state.");
    }

    // ---------- Void ----------

    [Fact]
    public void Void_Should_Set_Status_To_Voided_When_CreditNote_Is_Issued_And_Has_No_Allocations()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        creditNote.Void();

        // Assert
        creditNote.Status.Should().Be(CreditNoteStatus.Voided);
    }

    [Fact]
    public void Void_Should_Throw_DomainException_When_CreditNote_Is_Not_Issued()
    {
        // Arrange
        var creditNote = CreateDraftCreditNote("CAD", 100m);

        // Act
        Action act = () => creditNote.Void();

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation allowed only when credit note is in Issued state.");
    }

    [Fact]
    public void Void_Should_Throw_DomainException_When_CreditNote_Has_Allocations()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        creditNote.AllocateToInvoice(NewInvoiceId(), Money.Create("CAD", 40m), Today());

        // Act
        Action act = () => creditNote.Void();

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Cannot void a credit note that has been applied to invoices.");
    }

    // ---------- AllocateToInvoice ----------

    [Fact]
    public void AllocateToInvoice_Should_Add_Allocation_And_Reduce_RemainingAmount_When_Amount_Is_Valid()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        var invoiceId = NewInvoiceId();
        var amount = Money.Create("CAD", 40m);
        var allocatedOn = Today();

        // Act
        creditNote.AllocateToInvoice(invoiceId, amount, allocatedOn);

        // Assert
        creditNote.Allocations.Should().HaveCount(1);
        creditNote.AmountApplied.Should().Be(Money.Create("CAD", 40m));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 60m));

        var allocation = creditNote.Allocations.Should().ContainSingle().Subject;
        allocation.InvoiceId.Should().Be(invoiceId);
        allocation.Amount.Should().Be(amount);
        allocation.AppliedOn.Should().Be(allocatedOn);
    }

    [Fact]
    public void AllocateToInvoice_Should_Allow_Multiple_Allocations_To_Different_Invoices()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        creditNote.AllocateToInvoice(NewInvoiceId(), Money.Create("CAD", 30m), Today());
        creditNote.AllocateToInvoice(NewInvoiceId(), Money.Create("CAD", 20m), Today());

        // Assert
        creditNote.Allocations.Should().HaveCount(2);
        creditNote.AmountApplied.Should().Be(Money.Create("CAD", 50m));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 50m));
    }

    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_CreditNote_Is_Not_Issued()
    {
        // Arrange
        var creditNote = CreateDraftCreditNote("CAD", 100m);

        // Act
        Action act = () => creditNote.AllocateToInvoice(
            NewInvoiceId(),
            Money.Create("CAD", 10m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation allowed only when credit note is in Issued state.");
    }

    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Amount_Exceeds_RemainingAmount()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        creditNote.AllocateToInvoice(NewInvoiceId(), Money.Create("CAD", 60m), Today());

        // Act
        Action act = () => creditNote.AllocateToInvoice(
            NewInvoiceId(),
            Money.Create("CAD", 50m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Cannot allocate more than remaining credit.");
    }

    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Amount_Is_Zero()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        Action act = () => creditNote.AllocateToInvoice(
            NewInvoiceId(),
            Money.Create("CAD", 0m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Allocation amount must be greater than zero.");
    }

    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Amount_Is_Negative()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        Money negativeAmount = Money.Create("CAD", 5m).Negate();
        Action act = () => creditNote.AllocateToInvoice(
            NewInvoiceId(),
            negativeAmount,
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Allocation amount must be greater than zero.*");
    }

    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_AppliedOn_Is_Default()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);

        // Act
        Action act = () => creditNote.AllocateToInvoice(
            NewInvoiceId(),
            Money.Create("CAD", 10m),
            default);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("AppliedOn is required.");
    }

    // ---------- RemoveAllocation ----------

    [Fact]
    public void RemoveAllocation_Should_Remove_All_Allocations_For_Invoice_And_Restore_RemainingAmount()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        var invoiceId1 = NewInvoiceId();
        var invoiceId2 = NewInvoiceId();

        creditNote.AllocateToInvoice(invoiceId1, Money.Create("CAD", 20m), Today());
        creditNote.AllocateToInvoice(invoiceId1, Money.Create("CAD", 15m), Today());
        creditNote.AllocateToInvoice(invoiceId2, Money.Create("CAD", 25m), Today());

        // Act
        creditNote.RemoveAllocation(invoiceId1);

        // Assert
        creditNote.Allocations.Should().HaveCount(1);
        creditNote.Allocations.Should().OnlyContain(x => x.InvoiceId == invoiceId2);
        creditNote.AmountApplied.Should().Be(Money.Create("CAD", 25m));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 75m));
    }

    [Fact]
    public void RemoveAllocation_Should_Throw_DomainException_When_InvoiceId_Has_No_Allocation()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        creditNote.AllocateToInvoice(NewInvoiceId(), Money.Create("CAD", 25m), Today());

        // Act
        Action act = () => creditNote.RemoveAllocation(NewInvoiceId());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("No allocation exists for this payment.");
    }

    [Fact]
    public void RemoveAllocation_Should_Remove_Single_Allocation_When_Only_One_Exists_For_Invoice()
    {
        // Arrange
        var creditNote = CreateIssuedCreditNote("CAD", 100m);
        var invoiceId = NewInvoiceId();

        creditNote.AllocateToInvoice(invoiceId, Money.Create("CAD", 35m), Today());

        // Act
        creditNote.RemoveAllocation(invoiceId);

        // Assert
        creditNote.Allocations.Should().BeEmpty();
        creditNote.AmountApplied.Should().Be(Money.Zero("CAD"));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 100m));
    }

    // ---------- Helpers ----------

    private static CreditNote CreateDraftCreditNote(string currency, decimal amount)
    {
        return CreditNote.Create(NewLeaseId(), Money.Create(currency, amount));
    }

    private static CreditNote CreateIssuedCreditNote(string currency, decimal amount)
    {
        var creditNote = CreateDraftCreditNote(currency, amount);
        creditNote.Issue(Today());
        return creditNote;
    }

    private static LeaseId NewLeaseId() => new(Guid.NewGuid());

    private static InvoiceId NewInvoiceId() => new(Guid.NewGuid());

    private static DateOnly Today() => new(2026, 3, 7);
}