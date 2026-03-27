using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentsByTenant;

public sealed record GetPaymentsByTenantQuery(
    TenantId TenantId,
    string? Reference,
    PaymentMethod? Method,
    DateOnly ReceivedOn,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<PaymentResponse>>;
