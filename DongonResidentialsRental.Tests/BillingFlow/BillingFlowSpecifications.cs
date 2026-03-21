using AwesomeAssertions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using DomainPayment = DongonResidentialsRental.Domain.Payment.Payment;
using DomainInvoice = DongonResidentialsRental.Domain.Invoice.Invoice;

namespace DongonResidentialsRental.Tests.Domain.BillingFlow;

public sealed class BillingFlowSpecifications
{
    // This test class focuses on the core billing flows of applying payments and credits to invoices.
    // ---------- Apply Payment Flow ----------

    [Fact]
    public void ApplyPaymentFlow_Should_Reduce_Invoice_Balance_And_Payment_Remaining_When_Amount_Is_Valid()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 40m);

        // Act
        payment.ApplyToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Assert
        invoice.AmountPaid.Should().Be(Money.Create("CAD", 40m));
        invoice.Balance.Should().Be(Money.Create("CAD", 60m));
        payment.AllocatedAmount.Should().Be(Money.Create("CAD", 40m));
        payment.RemainingAmount.Should().Be(Money.Create("CAD", 60m));
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Fully_Pay_Invoice_When_Allocated_Amount_Equals_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 100m);

        // Act
        payment.ApplyToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Assert
        invoice.AmountPaid.Should().Be(Money.Create("CAD", 100m));
        invoice.Balance.Should().Be(Money.Zero("CAD"));
        payment.AllocatedAmount.Should().Be(Money.Create("CAD", 100m));
        payment.RemainingAmount.Should().Be(Money.Zero("CAD"));
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Throw_When_Payment_Exceeds_Invoice_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 200m);

        // Act
        payment.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 120m), Today());
        Action act = () => invoice.ApplyPayment(payment.PaymentId, Money.Create("CAD", 120m), Today());

        // Assert
        act.Should().ThrowExactly<DomainException>();
    }

    // ---------- Apply Credit Flow ----------

    [Fact]
    public void ApplyCreditFlow_Should_Reduce_Invoice_Balance_And_Credit_Remaining_When_Amount_Is_Valid()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var creditNote = CreateIssuedCreditNote(invoice.LeaseId, "CAD", 100m);
        var amount = Money.Create("CAD", 40m);

        // Act
        creditNote.ApplyToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyCredit(creditNote.CreditNoteId, amount, Today());

        // Assert
        invoice.AmountCredited.Should().Be(Money.Create("CAD", 40m));
        invoice.Balance.Should().Be(Money.Create("CAD", 60m));
        creditNote.AmountApplied.Should().Be(Money.Create("CAD", 40m));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 60m));
    }

    [Fact]
    public void ApplyCreditFlow_Should_Fully_Credit_Invoice_When_Allocated_Amount_Equals_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var creditNote = CreateIssuedCreditNote(invoice.LeaseId, "CAD", 100m);
        var amount = Money.Create("CAD", 100m);

        // Act
        creditNote.ApplyToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyCredit(creditNote.CreditNoteId, amount, Today());

        // Assert
        invoice.AmountCredited.Should().Be(Money.Create("CAD", 100m));
        invoice.Balance.Should().Be(Money.Zero("CAD"));
        creditNote.AmountApplied.Should().Be(Money.Create("CAD", 100m));
        creditNote.RemainingAmount.Should().Be(Money.Zero("CAD"));
    }

    [Fact]
    public void ApplyCreditFlow_Should_Throw_When_Credit_Exceeds_Invoice_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var creditNote = CreateIssuedCreditNote(invoice.LeaseId, "CAD", 200m);

        // Act
        creditNote.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 120m), Today());
        Action act = () => invoice.ApplyCredit(creditNote.CreditNoteId, Money.Create("CAD", 120m), Today());

        // Assert
        act.Should().ThrowExactly<DomainException>();
    }

    [Fact]
    public void ApplyCreditFlow_Should_Allow_Applying_Credit_After_Partial_Payment()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 50m);
        var creditNote = CreateIssuedCreditNote(invoice.LeaseId, "CAD", 50m);

        // Act
        payment.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 30m), Today());
        invoice.ApplyPayment(payment.PaymentId, Money.Create("CAD", 30m), Today());

        creditNote.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 20m), Today());
        invoice.ApplyCredit(creditNote.CreditNoteId, Money.Create("CAD", 20m), Today());

        // Assert
        invoice.AmountPaid.Should().Be(Money.Create("CAD", 30m));
        invoice.AmountCredited.Should().Be(Money.Create("CAD", 20m));
        invoice.Balance.Should().Be(Money.Create("CAD", 50m));

        payment.RemainingAmount.Should().Be(Money.Create("CAD", 20m));
        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 30m));
    }

    [Fact]
    public void ApplyCreditFlow_Should_Allow_Applying_Payment_After_Partial_Credit()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 50m);
        var creditNote = CreateIssuedCreditNote(invoice.LeaseId, "CAD", 50m);

        // Act
        creditNote.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 20m), Today());
        invoice.ApplyCredit(creditNote.CreditNoteId, Money.Create("CAD", 20m), Today());

        payment.ApplyToInvoice(invoice.InvoiceId, Money.Create("CAD", 30m), Today());
        invoice.ApplyPayment(payment.PaymentId, Money.Create("CAD", 30m), Today());

        // Assert
        invoice.AmountCredited.Should().Be(Money.Create("CAD", 20m));
        invoice.AmountPaid.Should().Be(Money.Create("CAD", 30m));
        invoice.Balance.Should().Be(Money.Create("CAD", 50m));

        creditNote.RemainingAmount.Should().Be(Money.Create("CAD", 30m));
        payment.RemainingAmount.Should().Be(Money.Create("CAD", 20m));
    }

    // ---------- Helpers ----------

    private static DomainInvoice CreateIssuedInvoiceWithLine(string currency, decimal amount)
    {
        var invoice = DomainInvoice.Create(
            invoiceNumber: NewInvoiceNumber(),
            leaseId: NewLeaseId(),
            BillingPeriod.Create(
                 from: Today(),
                 to: Today().AddDays(30)),
            dueDate: Today().AddDays(45),
            currency: currency);

        invoice.AddLine("Rent", 1, Money.Create(currency, amount), InvoiceLineType.Rent);
        invoice.Issue(Today());

        return invoice;
    }

    private static DomainPayment CreatePayment(string currency, decimal amount)
    {
        return DomainPayment.Create(
            tenantId: NewTenantId(),
            amount: Money.Create(currency, amount),
            receivedOn: Today(),
            reference: "PAY-001",
            method: PaymentMethod.Cash);
    }

    private static CreditNote CreateIssuedCreditNote(LeaseId leaseId, string currency, decimal amount)
    {
        var creditNote = CreditNote.Create(leaseId, Money.Create(currency, amount));
        creditNote.Issue(Today());
        return creditNote;
    }

    private static LeaseId NewLeaseId() => new(Guid.NewGuid());
    private static TenantId NewTenantId() => new(Guid.NewGuid());
    private static DateOnly Today() => new(2026, 3, 7);
    private static string NewInvoiceNumber() => "INV-2026-000123";
}