using DongonResidentialsRental.Domain.Payment;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentDetailsQuery;

public sealed record PaymentDetailsResponse(
    Guid PaymentId,
    string TenantName,
    decimal Amount,
    string Currency,
    DateOnly ReceivedOn,
    string? Reference,
    PaymentMethod Method,
    IReadOnlyList<PaymentAllocationResponse> Allocations);
