using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Payments.Commands.ReversePayment;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Payments.Commands.ReversePayment;

public sealed class ReversePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ReversePaymentCommandHandler _handler;

    public ReversePaymentCommandHandlerTests()
    {
        _handler = new ReversePaymentCommandHandler(
            _paymentRepository,
            _invoiceRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var command = new ReversePaymentCommand(
            NewPaymentId(),
            NewInvoiceId(),
            "Duplicate payment");

        _invoiceRepository
            .GetByIdAsync(command.InvoiceId)
            .Returns((Invoice?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{command.InvoiceId}*");

        await _paymentRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<PaymentId>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Payment_Does_Not_Exist()
    {
        // Arrange
        var invoice = CreateIssuedInvoiceWithAppliedPayment(
            out _,
            appliedAmount: 200m,
            currency: "CAD",
            appliedOn: new DateOnly(2026, 3, 20));

        var command = new ReversePaymentCommand(
            NewPaymentId(),
            invoice.InvoiceId,
            "Duplicate payment");

        _invoiceRepository
            .GetByIdAsync(command.InvoiceId)
            .Returns(invoice);

        _paymentRepository
            .GetByIdAsync(command.PaymentId)
            .Returns((Payment?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{command.PaymentId}*");
    }

    [Fact]
    public async Task Handle_Should_Reverse_Payment_And_Remove_Invoice_Allocation_When_Request_Is_Valid()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 9, 0, 0);
        var reversedOn = DateOnly.FromDateTime(today);
        const string reason = "Duplicate payment";

        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var payment = CreatePayment(amount: 500m, currency: "CAD");

        var appliedAmount = Money.Create("CAD", 200m);
        var appliedOn = new DateOnly(2026, 3, 20);

        payment.ApplyToInvoice(invoice.InvoiceId, appliedAmount, appliedOn);
        invoice.ApplyPayment(payment.PaymentId, appliedAmount, appliedOn);

        var command = new ReversePaymentCommand(
            payment.PaymentId,
            invoice.InvoiceId,
            reason);

        _dateTimeProvider.Today.Returns(today);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        _paymentRepository
            .GetByIdAsync(payment.PaymentId)
            .Returns(payment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        payment.Status.Should().Be(PaymentStatus.Reversed);
        payment.ReversedOn.Should().Be(reversedOn);

        invoice.Allocations.Should().NotContain(x => x.PaymentId == payment.PaymentId);
        invoice.AmountPaid.Amount.Should().Be(0m);
        invoice.Balance.Amount.Should().Be(1200m);
    }

    [Fact]
    public async Task Handle_Should_Use_Today_From_DateTimeProvider_When_Reversing_Payment()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 14, 15, 0);
        var expectedDate = DateOnly.FromDateTime(today);

        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var payment = CreatePayment(amount: 500m, currency: "CAD");

        var appliedAmount = Money.Create("CAD", 200m);
        var appliedOn = new DateOnly(2026, 3, 20);

        payment.ApplyToInvoice(invoice.InvoiceId, appliedAmount, appliedOn);
        invoice.ApplyPayment(payment.PaymentId, appliedAmount, appliedOn);

        var command = new ReversePaymentCommand(
            payment.PaymentId,
            invoice.InvoiceId,
            "Duplicate payment");

        _dateTimeProvider.Today.Returns(today);

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        _paymentRepository
            .GetByIdAsync(payment.PaymentId)
            .Returns(payment);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.ReversedOn.Should().Be(expectedDate);
    }

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

    private static PaymentId NewPaymentId() => new PaymentId(Guid.NewGuid());
    private static InvoiceId NewInvoiceId() => new InvoiceId(Guid.NewGuid());
    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());
    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());

    private static Payment CreatePayment(decimal amount, string currency)
    {
        return Payment.Create(
            tenantId: NewTenantId(),
            amount: Money.Create(currency, amount),
            receivedOn: new DateOnly(2026, 3, 20),
            method: PaymentMethod.Cash,
            reference: null);
    }

    private static Invoice CreateIssuedInvoiceWithAppliedPayment(
        out Payment payment,
        decimal appliedAmount,
        string currency,
        DateOnly appliedOn)
    {
        var invoice = CreateIssuedInvoice(1200m, currency);
        payment = CreatePayment(500m, currency);

        var amount = Money.Create(currency, appliedAmount);

        payment.ApplyToInvoice(invoice.InvoiceId, amount, appliedOn);
        invoice.ApplyPayment(payment.PaymentId, amount, appliedOn);

        return invoice;
    }
}