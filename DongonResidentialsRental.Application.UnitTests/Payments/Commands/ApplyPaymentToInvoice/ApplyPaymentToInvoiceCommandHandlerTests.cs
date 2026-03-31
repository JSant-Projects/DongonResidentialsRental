using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Payments.Commands.ApplyPaymentToInvoice;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Payments.Commands.ApplyPaymentToInvoice;

public sealed class ApplyPaymentToInvoiceCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    private readonly ApplyPaymentToInvoiceCommandHandler _handler;

    public ApplyPaymentToInvoiceCommandHandlerTests()
    {
        _handler = new ApplyPaymentToInvoiceCommandHandler(
            _paymentRepository,
            _invoiceRepository,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Invoice_Does_Not_Exist()
    {
        // Arrange
        var command = new ApplyPaymentToInvoiceCommand(
            NewPaymentId(),
            NewInvoiceId(),
            200m);

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
        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");

        var command = new ApplyPaymentToInvoiceCommand(
            NewPaymentId(),
            invoice.InvoiceId,
            200m);

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
    public async Task Handle_Should_Apply_Payment_To_Both_Payment_And_Invoice_When_Request_Is_Valid()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27, 10, 30, 0);
        var expectedAppliedOn = DateOnly.FromDateTime(today);

        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var payment = CreatePayment(amount: 500m, currency: "CAD");

        var command = new ApplyPaymentToInvoiceCommand(
            payment.PaymentId,
            invoice.InvoiceId,
            200m);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

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

        payment.Allocations.Should().HaveCount(1);
        payment.Allocations.Single().InvoiceId.Should().Be(invoice.InvoiceId);
        payment.Allocations.Single().Amount.Amount.Should().Be(200m);
        payment.Allocations.Single().Amount.Currency.Should().Be("CAD");
        payment.Allocations.Single().AllocatedOn.Should().Be(expectedAppliedOn);

        invoice.Allocations.Should().HaveCount(1);
        invoice.Allocations.Single().PaymentId.Should().Be(payment.PaymentId);
        invoice.Allocations.Single().Amount.Amount.Should().Be(200m);
        invoice.Allocations.Single().Amount.Currency.Should().Be("CAD");
        invoice.Allocations.Single().AppliedOn.Should().Be(expectedAppliedOn);

        payment.RemainingAmount.Amount.Should().Be(300m);
        invoice.AmountPaid.Amount.Should().Be(200m);
        invoice.Balance.Amount.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_Should_Use_Payment_Currency_When_Creating_Amount()
    {
        // Arrange
        var today = new DateTime(2026, 3, 27);
        var invoice = CreateIssuedInvoice(totalAmount: 1200m, currency: "CAD");
        var payment = CreatePayment(amount: 500m, currency: "CAD");

        var command = new ApplyPaymentToInvoiceCommand(
            payment.PaymentId,
            invoice.InvoiceId,
            150m);

        _dateTimeProvider.Today.Returns(DateOnly.FromDateTime(today));

        _invoiceRepository
            .GetByIdAsync(invoice.InvoiceId)
            .Returns(invoice);

        _paymentRepository
            .GetByIdAsync(payment.PaymentId)
            .Returns(payment);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Allocations.Single().Amount.Currency.Should().Be(payment.Amount.Currency);
        invoice.Allocations.Single().Amount.Currency.Should().Be(payment.Amount.Currency);
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
    private static LeaseId NewLeaseId() => new LeaseId(Guid.NewGuid());
    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());

    private static Payment CreatePayment(decimal amount, string currency)
    {
        return Payment.Create(
            tenantId: NewTenantId(),
            amount: Money.Create(currency, amount),
            receivedOn: new DateOnly(2026, 3, 20),
            method: PaymentMethod.Cash,
            reference: null);
    }
}
