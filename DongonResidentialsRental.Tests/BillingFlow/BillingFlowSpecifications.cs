using AwesomeAssertions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using DomainPayment = DongonResidentialsRental.Domain.Payment.Payment;

namespace DongonResidentialsRental.Tests.BillingFlow;

public sealed class BillingFlowSpecifications
{
    // ---------- Apply Payment Flow ----------

    [Fact]
    public void ApplyPaymentFlow_Should_Reduce_Invoice_Balance_And_Payment_Remaining_When_Amount_Is_Valid()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 40m);

        // Act
        payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Assert
        payment.Allocations.Should().HaveCount(1);
        payment.AllocatedAmount.Amount.Should().Be(40m);
        payment.RemainingAmount.Amount.Should().Be(60m);

        invoice.AmountPaid.Amount.Should().Be(40m);
        invoice.Balance.Amount.Should().Be(60m);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Fully_Pay_Invoice_When_Allocation_Equals_Invoice_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 100m);

        // Act
        payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Assert
        payment.RemainingAmount.Amount.Should().Be(0m);
        invoice.AmountPaid.Amount.Should().Be(100m);
        invoice.Balance.Amount.Should().Be(0m);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Allow_Partial_Payment_When_Payment_Amount_Is_Less_Than_Invoice_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 30m);
        var amount = Money.Create("CAD", 30m);

        // Act
        payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Assert
        payment.RemainingAmount.Amount.Should().Be(0m);
        invoice.AmountPaid.Amount.Should().Be(30m);
        invoice.Balance.Amount.Should().Be(70m);

        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Throw_When_Payment_Allocation_Exceeds_Payment_Remaining()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 200m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 120m);

        // Act
        Action act = () =>
        {
            payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
            invoice.ApplyPayment(payment.PaymentId, amount, Today());
        };

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Allocation amount cannot exceed remaining payment amount.");

        payment.Allocations.Should().BeEmpty();
        payment.RemainingAmount.Amount.Should().Be(100m);

        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(200m);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Throw_When_Payment_Amount_Exceeds_Invoice_Balance()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 150m);
        var amount = Money.Create("CAD", 120m);

        // Act
        Action act = () =>
        {
            payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
            invoice.ApplyPayment(payment.PaymentId, amount, Today());
        };

        // Assert
        act.Should().ThrowExactly<DomainException>();

        payment.Allocations.Should().HaveCount(1);
        payment.AllocatedAmount.Amount.Should().Be(120m);

        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(100m);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Throw_When_Currency_Does_Not_Match_Invoice_And_Payment()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("USD", 100m);
        var amount = Money.Create("USD", 40m);

        // Act
        Action act = () =>
        {
            payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
            invoice.ApplyPayment(payment.PaymentId, amount, Today());
        };

        // Assert
        act.Should().ThrowExactly<DomainException>();

        payment.Allocations.Should().HaveCount(1);
        payment.RemainingAmount.Amount.Should().Be(60m);

        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(100m);
    }

    [Fact]
    public void ApplyPaymentFlow_Should_Not_Allow_Allocation_To_Reversed_Payment()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        payment.Reverse(Today(), "Voided");

        var amount = Money.Create("CAD", 50m);

        // Act
        Action act = () =>
        {
            payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
            invoice.ApplyPayment(payment.PaymentId, amount, Today());
        };

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation allowed only when payment is in Received state.");

        payment.Allocations.Should().BeEmpty();
        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(100m);
    }

    [Fact]
    public void RemoveAllocationFlow_Should_Restore_Invoice_Balance_And_Payment_Remaining_When_Allocation_Is_Removed()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithLine("CAD", 100m);
        var payment = CreatePayment("CAD", 100m);
        var amount = Money.Create("CAD", 60m);

        payment.AllocateToInvoice(invoice.InvoiceId, amount, Today());
        invoice.ApplyPayment(payment.PaymentId, amount, Today());

        // Act
        invoice.RemoveAllocation(payment.PaymentId);

        // Assert
        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(100m);
    }


    // ---------- Helpers ----------

    private static DomainPayment CreatePayment(string currency, decimal amount)
    {
        return DomainPayment.Create(
            NewTenantId(),
            Money.Create(currency, amount),
            Today(),
            PaymentMethod.Cash);
    }

    private static Invoice CreateIssuedInvoiceWithLine(string currency, decimal lineAmount)
    {
        var leaseId = NewLeaseId();

        var invoice = Invoice.Create(
            leaseId,
            BillingPeriod.Create(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31)),
            currency);

        invoice.AddLine(
            "Monthly Rent",
            1,
            Money.Create(currency, lineAmount),
            InvoiceLineType.Rent);

        invoice.Issue(new DateOnly(2026, 4, 5));

        return invoice;
    }

    private static TenantId NewTenantId()
        => new TenantId(Guid.NewGuid());

    private static DateOnly Today()
        => DateOnly.FromDateTime(DateTime.UtcNow);

    private static LeaseId NewLeaseId()
    => new LeaseId(Guid.NewGuid());
}
