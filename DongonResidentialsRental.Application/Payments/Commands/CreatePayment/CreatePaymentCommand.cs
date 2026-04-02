using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Commands.CreatePayment;

public sealed record CreatePaymentCommand(
    TenantId TenantId,
    decimal Amount,
    string Currency,
    DateOnly ReceivedOn,
    PaymentMethod PaymentMethod,
    string? Reference) : ICommand<PaymentId>;
