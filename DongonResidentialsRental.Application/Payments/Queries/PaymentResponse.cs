using DongonResidentialsRental.Domain.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries;

public sealed record PaymentResponse(
    Guid PaymentId,
    string TenantName,
    decimal Amount,
    string Currency,
    DateOnly ReceivedOn,
    string? Reference,
    PaymentMethod Method);
