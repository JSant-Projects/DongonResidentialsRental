using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Payments.Commands.CreatePayment;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();

    private readonly CreatePaymentCommandHandler _handler;
    public CreatePaymentCommandHandlerTests()
    {
        _handler = new CreatePaymentCommandHandler(_paymentRepository, _tenantRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Tenant_Does_Not_Exists()
    {
        // Arrange
        var tenantId = NewTenantId();
        var paymentAmount = Money.Create("CAD", 1200m);
        var receivedOn = new DateOnly(2026, 3, 20);

        var command = new CreatePaymentCommand(
            tenantId,
            paymentAmount.Amount,
            paymentAmount.Currency,
            receivedOn,
            PaymentMethod.Cash,
            null);

        _tenantRepository
            .GetByIdAsync(
                tenantId, 
                Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);
        // Act

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should()
            .ThrowExactlyAsync<NotFoundException>()
            .WithMessage($"*{command.TenantId}*");

        _paymentRepository
            .DidNotReceive()
            .Add(Arg.Any<Payment>());

    }

    [Fact]
    public async Task Handle_Should_Create_Payment_When_Request_Is_Valid()
    {
        // Arrange
        var tenant = CreateTenant("Jayson"); ;
        var paymentAmount = Money.Create("CAD", 1200m);
        var receivedOn = new DateOnly(2026, 3, 20);

        var command = new CreatePaymentCommand(
            tenant.TenantId,
            paymentAmount.Amount,
            paymentAmount.Currency,
            receivedOn,
            PaymentMethod.Cash,
            null);

        _tenantRepository
            .GetByIdAsync(
                tenant.TenantId, 
                Arg.Any<CancellationToken>())
            .Returns(tenant);

        Payment? addedPayment = null;
        _paymentRepository
            .When(x => x.Add(Arg.Any<Payment>()))
            .Do(callInfo => addedPayment = callInfo.Arg<Payment>());
       
        // Act

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert

        addedPayment.Should().NotBeNull();
        addedPayment.PaymentId.Should().Be(result);
        addedPayment.TenantId.Should().Be(tenant.TenantId);
        addedPayment.Amount.Amount.Should().Be(paymentAmount.Amount);
        addedPayment.Amount.Currency.Should().Be(paymentAmount.Currency);
        addedPayment.ReceivedOn.Should().Be(receivedOn);
    }

    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());

    private static Tenant CreateTenant(
        string firstName = "John",
        string lastName = "Doe",
        string email = "johndoe@testemail.com",
        string phoneNumber = "09123456791")
    {
        return Tenant.Create(
            PersonalInfo.Create(firstName, lastName),
            ContactInfo.Create(
                email: Email.Create(email),
                phoneNumber: PhoneNumber.Create(phoneNumber)));
    }

}
