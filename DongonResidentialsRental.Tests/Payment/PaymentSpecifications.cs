using AwesomeAssertions;
using DomainPayment = DongonResidentialsRental.Domain.Payment.Payment;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Tests.Payment;

public class PaymentSpecifications
{
    // ---------- Create ----------

    [Fact]
    public void Create_Should_Create_Payment_When_Data_Is_Valid()
    {
        // Arrange
        var tenantId = NewTenantId();
        var amount = Money.Create("CAD", 100m);
        var receivedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var payment = DomainPayment.Create(
            tenantId,
            amount,
            receivedOn,
            PaymentMethod.Cash);

        // Assert
        payment.Should().NotBeNull();
        payment.Status.Should().Be(PaymentStatus.Received);
        payment.Amount.Amount.Should().Be(100m);
        payment.RemainingAmount.Amount.Should().Be(100m);
        payment.Allocations.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Throw_DomainException_When_Amount_Is_Zero()
    {
        // Arrange
        var tenantId = NewTenantId();
        var amount = Money.Create("CAD", 0m);

        // Act
        Action act = () => DomainPayment.Create(
            tenantId,
            amount,
            DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod.Cash);

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Payment amount must be greater than zero.");
    }


    // ---------- AllocateToInvoice ----------

    [Fact]
    public void AllocateToInvoice_Should_Add_Allocation_When_Valid()
    {
        // Arrange
        var payment = CreatePayment(100m);
        var invoiceId = NewInvoiceId();

        // Act
        payment.AllocateToInvoice(
            invoiceId,
            Money.Create("CAD", 40m),
            Today());

        // Assert
        payment.Allocations.Should().HaveCount(1);
        payment.AllocatedAmount.Amount.Should().Be(40m);
        payment.RemainingAmount.Amount.Should().Be(60m);
    }


    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Allocation_Exceeds_Remaining()
    {
        // Arrange
        var payment = CreatePayment(100m);
        var invoiceId = NewInvoiceId();

        // Act
        Action act = () => payment.AllocateToInvoice(
            invoiceId,
            Money.Create("CAD", 150m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Allocation amount cannot exceed remaining payment amount.");
    }


    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Currency_Does_Not_Match()
    {
        // Arrange
        var payment = CreatePayment(100m);
        var invoiceId = NewInvoiceId();

        // Act
        Action act = () => payment.AllocateToInvoice(
            invoiceId,
            Money.Create("USD", 50m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Allocation currency must match payment currency.");
    }


    [Fact]
    public void AllocateToInvoice_Should_Throw_DomainException_When_Payment_Is_Reversed()
    {
        // Arrange
        var payment = CreatePayment(100m);
        var invoiceId = NewInvoiceId();

        payment.Reverse(Today(), "Refunded");

        // Act
        Action act = () => payment.AllocateToInvoice(
            invoiceId,
            Money.Create("CAD", 20m),
            Today());

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Operation allowed only when payment is in Received state.");
    }


    // ---------- Reverse ----------

    [Fact]
    public void Reverse_Should_Set_Status_To_Reversed()
    {
        // Arrange
        var payment = CreatePayment(100m);

        // Act
        payment.Reverse(Today(), "Refunded");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Reversed);
        payment.ReversedOn.Should().Be(Today());
        payment.ReversalReason.Should().Be("Refunded");
    }


    [Fact]
    public void Reverse_Should_Throw_DomainException_When_Already_Reversed()
    {
        // Arrange
        var payment = CreatePayment(100m);
        payment.Reverse(Today(), "Refunded");

        // Act
        Action act = () => payment.Reverse(Today(), "Another reason");

        // Assert
        act.Should().ThrowExactly<DomainException>()
            .WithMessage("Payment is already reversed.");
    }



    // ---------- Helpers ----------

    private static DomainPayment CreatePayment(decimal amount)
    {
        return DomainPayment.Create(
            NewTenantId(),
            Money.Create("CAD", amount),
            Today(),
            PaymentMethod.Cash);
    }

    private static TenantId NewTenantId()
        => new TenantId(Guid.NewGuid());

    private static InvoiceId NewInvoiceId()
        => new InvoiceId(Guid.NewGuid());

    private static DateOnly Today()
        => DateOnly.FromDateTime(DateTime.UtcNow);
}
