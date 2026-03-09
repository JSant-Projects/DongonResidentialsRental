using AwesomeAssertions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Invoice.Events;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using System;
using Xunit;
using DomainInvoice = DongonResidentialsRental.Domain.Invoice.Invoice;

namespace DongonResidentialsRental.Domain.Tests.Invoice;

public sealed class InvoiceTests
{
    // ---------- Create ----------
    [Fact]
    public void Create_Should_Create_Draft_Invoice_With_Normalized_Currency()
    {
        // Act
        var invoice = CreateDraftInvoice(currency: " cad ");

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.IssuedOn.Should().BeNull();
        invoice.Currency.Should().Be("CAD");
        invoice.Lines.Should().BeEmpty();

        invoice.Total.Should().Be(Zero("CAD"));
        invoice.AmountPaid.Should().Be(Zero("CAD"));
        invoice.Balance.Should().Be(Zero("CAD"));
    }

    [Fact]
    public void Create_Should_Add_DomainEvent()
    {
        // Act
        var invoice = CreateDraftInvoice();

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoiceDraftCreatedDomainEvent>()
            .Single();
        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.LeaseId.Should().Be(invoice.LeaseId);
        domainEvent.BillingPeriod.Should().Be(invoice.BillingPeriod);
    }

    // ---------- AddLine ----------
    [Fact]
    public void AddLine_Should_Add_Line_When_Draft_And_Currency_Matches()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");

        // Act
        AddLine(invoice, "Rent", qty: 1, unitPrice: 100m, type: InvoiceLineType.Rent);

        // Assert
        invoice.Lines.Should().HaveCount(1);
        invoice.Total.Should().Be(CreateMoney("CAD", 100m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 100m));
    }

    [Fact]
    public void AddLine_Should_Throw_ArgumentException_When_Quantity_Is_Zero()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");

        // Act
        Action act = () => AddLine(invoice, "Rent", qty: 0, unitPrice: 100m, type: InvoiceLineType.Rent);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("Quantity must be at least 1.*");
    }

    [Fact]
    public void AddLine_Should_Throw_ArgumentException_When_UnitPrice_Is_Null()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");

        // Act
        Action act = () => invoice.AddLine("Rent", 1, null!, InvoiceLineType.Rent);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Unit price cannot be null*");
    }

    [Fact]
    public void AddLine_Should_Throw_DomainException_When_Not_Draft()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine(currency: "CAD", lineTotal: 100m);

        // Act
        var act = () => AddLine(invoice, "Late penalty", qty: 1, unitPrice: 10m, type: InvoiceLineType.Penalty);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Operation allowed only when invoice is in Draft state.");
    }

    [Fact]
    public void AddLine_Should_Throw_DomainException_When_Line_Currency_Does_Not_Match_Invoice_Currency()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");

        // Act
        var act = () => invoice.AddLine("Rent", 1, CreateMoney("USD", 100m), InvoiceLineType.Rent);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Line currency must match invoice currency.");
    }

    // ---------- Issue ----------
    [Fact]
    public void Issue_Should_Throw_DomainException_When_No_Lines()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");

        // Act
        var act = () => Issue(invoice);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot issue an invoice with no lines.");
    }

    [Fact]
    public void Issue_Should_Set_Status_To_Issued_And_Set_IssuedOn()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");
        AddLine(invoice, "Rent", 1, 100m, InvoiceLineType.Rent);

        // Act
        Issue(invoice);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.IssuedOn.Should().Be(IssuedOn());
    }

    [Fact]
    public void Issue_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");
        AddLine(invoice, "Rent", 1, 100m, InvoiceLineType.Rent);

        // Act
        Issue(invoice);

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoiceIssuedDomainEvent>()
            .Single();
        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.LeaseId.Should().Be(invoice.LeaseId);
        domainEvent.IssuedDate.Should().Be(IssuedOn());
    }   

    // ---------- ApplyPayment ----------
    [Fact]
    public void ApplyPayment_Should_Throw_DomainException_When_Not_Issued()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");
        AddLine(invoice, "Rent", 1, 100m, InvoiceLineType.Rent);

        // Act
        var act = () => ApplyPayment(invoice, amount: 50m);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation is not allowed when invoice is in Draft or Cancelled state.");
    }

    [Fact]
    public void ApplyPayment_Should_Throw_When_DomainException_Payment_Currency_Does_Not_Match()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        var act = () => invoice.ApplyPayment(NewPaymentId(), CreateMoney("USD", 10m), AppliedOn());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Payment currency must match invoice currency.");
    }

    [Fact]
    public void ApplyPayment_Should_Throw_DomainException_When_Payment_Exceeds_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        var act = () => ApplyPayment(invoice, amount: 101m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Payment exceeds remaining balance.");
    }

    [Fact]
    public void ApplyPayment_Should_Throw_DomainException_When_Payment_Is_Zero()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        var act = () => ApplyPayment(invoice, amount: 0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Allocation amount must be greater than zero.");
    }

    [Fact]
    public void ApplyPayment_Should_Throw_DomainException_When_Same_PaymentId_Is_Applied_Twice()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var paymentId = NewPaymentId();

        // Act
        var act = () => invoice.ApplyPayment(paymentId, CreateMoney("CAD", 50m), AppliedOn());
        act += () => invoice.ApplyPayment(paymentId, CreateMoney("CAD", 30m), AppliedOn());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Payment has already been applied to this invoice.");
    }

    [Fact]
    public void ApplyPayment_Should_Reduce_Balance_When_Payment_Is_Less_Than_Total()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyPayment(invoice, amount: 40m);

        // Assert
        invoice.AmountPaid.Should().Be(CreateMoney("CAD", 40m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 60m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void ApplyPayment_Should_Zero_Balance_When_Invoice_Is_Fully_Paid()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyPayment(invoice, amount: 100m);

        // Assert
        invoice.AmountPaid.Should().Be(CreateMoney("CAD", 100m));
        invoice.Balance.Should().Be(Zero("CAD"));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void ApplyPayment_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var paymentId = NewPaymentId();

        // Act
        invoice.ApplyPayment(paymentId, CreateMoney("CAD", 40m), AppliedOn());

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoicePaymentAppliedDomainEvent>()
            .Single();
        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.PaymentId.Should().Be(paymentId);
        domainEvent.Amount.Should().Be(CreateMoney("CAD", 40m));
        domainEvent.AppliedOn.Should().Be(AppliedOn());
    }

    // ---------- Total ----------
    [Fact]
    public void Total_Should_Sum_Multiple_Lines()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");
        AddLine(invoice, "Rent", 1, 1000m, InvoiceLineType.Rent);
        AddLine(invoice, "Water", 2, 25m, InvoiceLineType.Water);
        AddLine(invoice, "Electricity", 1, 75m, InvoiceLineType.Electricity);

        // Assert
        invoice.Total.Should().Be(CreateMoney("CAD", 1125m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 1125m));
    }

    // ---------- RemoveAllocation ----------
    [Fact]
    public void RemoveAllocation_Should_Throw_DomainException_When_PaymentId_Does_Not_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        ApplyPayment(invoice, amount: 30m);

        var unknownPaymentId = NewPaymentId();

        // Act
        Action act = () => invoice.RemoveAllocation(unknownPaymentId);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("No allocation exists for this payment.");
    }

    [Fact]
    public void RemoveAllocation_Should_Reduce_AmountPaid_When_Allocation_Is_Removed()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyPayment(invoice, amount: 30m);
        var paymentId = NewPaymentId();
        ApplyPaymentWithPaymentId(invoice, paymentId, amount: 60m);
        invoice.RemoveAllocation(paymentId);

        // Assert
        invoice.AmountPaid.Should().Be(CreateMoney("CAD", 30m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 70m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveAllocation_Should_Keep_Invoice_As_Issued_When_Other_Allocations_Still_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        ApplyPayment(invoice, amount: 30m);

        var paymentId = NewPaymentId();
        ApplyPaymentWithPaymentId(invoice, paymentId, amount: 60m);

        // Act
        invoice.RemoveAllocation(paymentId);

        // Assert
        invoice.AmountPaid.Should().Be(CreateMoney("CAD", 30m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 70m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveAllocation_Should_Keep_Status_As_Issued_When_Last_Allocation_Is_Removed()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        var paymentId = NewPaymentId();
        ApplyPaymentWithPaymentId(invoice, paymentId, amount: 100m);

        // Act
        invoice.RemoveAllocation(paymentId);

        // Assert
        invoice.AmountPaid.Should().Be(Zero("CAD"));
        invoice.Balance.Should().Be(CreateMoney("CAD", 100m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveAllocation_Should_Throw_DomainException_WhRemoveAllocation_Should_Throw_DomainException_When_PaymentId_Does_Not_Existen_Invoice_Is_Draft()
    {
        // Arrange
        var invoice = CreateDraftInvoiceWithLine("CAD", 100m);
        var paymentId = NewPaymentId();

        // Act
        Action act = () => invoice.RemoveAllocation(paymentId);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("No allocation exists for this payment.");
    }

    [Fact]
    public void RemoveAllocation_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var paymentId = NewPaymentId();
        ApplyPaymentWithPaymentId(invoice, paymentId, amount: 40m);

        // Act
        invoice.RemoveAllocation(paymentId);

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoicePaymentRemovedDomainEvent>()
            .Single();
        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.PaymentId.Should().Be(paymentId);
    }

    // ---------- ApplyCredit ----------
    [Fact]
    public void ApplyCredit_Should_Throw_DomainException_When_Not_Issued()
    {
        // Arrange
        var invoice = CreateDraftInvoice("CAD");
        AddLine(invoice, "Rent", 1, 100m, InvoiceLineType.Rent);

        // Act
        var act = () => ApplyCredit(invoice, amount: 50m);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation is not allowed when invoice is in Draft or Cancelled state.");
    }

    [Fact]
    public void ApplyCredit_Should_Throw_DomainException_When_Credit_Currency_Does_Not_Match()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        var act = () => invoice.ApplyCredit(NewCreditNoteId(), CreateMoney("USD", 10m), AppliedOn());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Credit currency must match invoice currency.");
    }

    [Fact]
    public void ApplyCredit_Should_Throw_DomainException_When_Credit_Exceeds_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        var act = () => ApplyCredit(invoice, amount: 101m);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Credit amount cannot exceed invoice balance.");
    }

    [Fact]
    public void ApplyCredit_Should_Reduce_Balance_When_Credit_Is_Less_Than_Total()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyCredit(invoice, amount: 40m);

        // Assert
        invoice.AmountCredited.Should().Be(CreateMoney("CAD", 40m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 60m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void ApplyCredit_Should_Zero_Balance_When_Invoice_Is_Fully_Credited()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyCredit(invoice, amount: 100m);

        // Assert
        invoice.AmountCredited.Should().Be(CreateMoney("CAD", 100m));
        invoice.Balance.Should().Be(Zero("CAD"));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void ApplyCredit_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var creditNoteId = NewCreditNoteId();

        // Act
        invoice.ApplyCredit(creditNoteId, CreateMoney("CAD", 40m), AppliedOn());

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoiceCreditAppliedDomainEvent>()
            .Single();

        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.CreditNoteId.Should().Be(creditNoteId);
        domainEvent.Amount.Should().Be(CreateMoney("CAD", 40m));
        domainEvent.AppliedOn.Should().Be(AppliedOn());
    }

    // ---------- RemoveCreditAllocation ----------
    [Fact]
    public void RemoveCreditAllocation_Should_Throw_DomainException_When_CreditNoteId_Does_Not_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        ApplyCredit(invoice, amount: 30m);

        var unknownCreditNoteId = NewCreditNoteId();

        // Act
        Action act = () => invoice.RemoveCreditAllocation(unknownCreditNoteId);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("No allocation exists for this credit note.");
    }

    [Fact]
    public void RemoveCreditAllocation_Should_Reduce_AmountCredited_When_Allocation_Is_Removed()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        ApplyCredit(invoice, amount: 30m);
        var creditNoteId = NewCreditNoteId();
        ApplyCreditWithCreditNoteId(invoice, creditNoteId, amount: 60m);
        invoice.RemoveCreditAllocation(creditNoteId);

        // Assert
        invoice.AmountCredited.Should().Be(CreateMoney("CAD", 30m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 70m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveCreditAllocation_Should_Keep_Invoice_As_Issued_When_Other_Credit_Allocations_Still_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        ApplyCredit(invoice, amount: 30m);

        var creditNoteId = NewCreditNoteId();
        ApplyCreditWithCreditNoteId(invoice, creditNoteId, amount: 60m);

        // Act
        invoice.RemoveCreditAllocation(creditNoteId);

        // Assert
        invoice.AmountCredited.Should().Be(CreateMoney("CAD", 30m));
        invoice.Balance.Should().Be(CreateMoney("CAD", 70m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveCreditAllocation_Should_Keep_Status_As_Issued_When_Last_Credit_Allocation_Is_Removed()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        var creditNoteId = NewCreditNoteId();
        ApplyCreditWithCreditNoteId(invoice, creditNoteId, amount: 100m);

        // Act
        invoice.RemoveCreditAllocation(creditNoteId);

        // Assert
        invoice.AmountCredited.Should().Be(Zero("CAD"));
        invoice.Balance.Should().Be(CreateMoney("CAD", 100m));
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void RemoveCreditAllocation_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var creditNoteId = NewCreditNoteId();
        ApplyCreditWithCreditNoteId(invoice, creditNoteId, amount: 40m);

        // Act
        invoice.RemoveCreditAllocation(creditNoteId);

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoiceCreditRemovedDomainEvent>()
            .Single();

        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
        domainEvent.CreditNoteId.Should().Be(creditNoteId);
    }

    // ---------- Cancel ----------
    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        invoice.Cancel();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Should_Throw_DomainException_When_Invoice_Has_Payments_Applied()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        ApplyPayment(invoice, amount: 50m);

        // Act
        var act = () => invoice.Cancel();

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Cannot cancel an invoice that has been paid.");
    }

    [Fact]
    public void Cancel_Should_Add_DomainEvent()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);

        // Act
        invoice.Cancel();

        // Assert
        var domainEvent = invoice.DomainEvents
            .OfType<InvoiceCancelledDomainEvent>()
            .Single();
        domainEvent.InvoiceId.Should().Be(invoice.InvoiceId);
    }

    // -------------------------
    // Helpers
    // -------------------------

    private static DomainInvoice CreateDraftInvoice(string currency = "CAD")
        => DomainInvoice.Create(NewLeaseId(), DefaultBillingPeriod(), DueDate(), currency);

    private static DomainInvoice CreateIssuedInvoiceWithLine(string currency, decimal lineTotal)
    {
        var invoice = CreateDraftInvoice(currency);
        AddLine(invoice, "Rent", 1, lineTotal, InvoiceLineType.Rent);
        Issue(invoice);
        return invoice;
    }

    private static DomainInvoice CreateDraftInvoiceWithLine(string currency, decimal lineTotal)
    {
        var invoice = CreateDraftInvoice(currency);
        AddLine(invoice, "Rent", 1, lineTotal, InvoiceLineType.Rent);
        return invoice;
    }

    private static void AddLine(DomainInvoice invoice, string description, int qty, decimal unitPrice, InvoiceLineType type)
        => invoice.AddLine(description, qty, CreateMoney(invoice.Currency, unitPrice), type);

    private static void Issue(DomainInvoice invoice) => invoice.Issue(IssuedOn());

    private static void ApplyPayment(DomainInvoice invoice, decimal amount)
        => invoice.ApplyPayment(NewPaymentId(), CreateMoney(invoice.Currency, amount), AppliedOn());

    private static void ApplyPaymentWithPaymentId(DomainInvoice invoice, PaymentId paymentId, decimal amount)
        => invoice.ApplyPayment(paymentId, CreateMoney(invoice.Currency, amount), AppliedOn());

    private static Money CreateMoney(string currency, decimal amount) => Money.Create(currency, amount);

    private static Money Zero(string currency) => Money.Zero(currency);

    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());

    private static PaymentId NewPaymentId() => new PaymentId(Guid.NewGuid());

    private static BillingPeriod DefaultBillingPeriod()
        => BillingPeriod.Create(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

    private static DateOnly IssuedOn() => new DateOnly(2026, 3, 4);

    private static DateOnly AppliedOn() => new DateOnly(2026, 3, 4);
    
    private static DateOnly DueDate() => new DateOnly(2026, 4, 15);

    private static void ApplyCredit(DomainInvoice invoice, decimal amount)
    => invoice.ApplyCredit(NewCreditNoteId(), CreateMoney(invoice.Currency, amount), AppliedOn());

    private static void ApplyCreditWithCreditNoteId(DomainInvoice invoice, CreditNoteId creditNoteId, decimal amount)
        => invoice.ApplyCredit(creditNoteId, CreateMoney(invoice.Currency, amount), AppliedOn());

    private static CreditNoteId NewCreditNoteId() => new CreditNoteId(Guid.NewGuid());
}