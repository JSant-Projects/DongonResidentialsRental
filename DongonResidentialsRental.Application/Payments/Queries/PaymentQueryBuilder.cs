using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries;

public static class PaymentQueryBuilder
{
    public static IQueryable<PaymentResponse> ApplyReceivedOnFilter(
        this IQueryable<PaymentResponse> query,
        DateOnly? receivedOn)
    {
        if (receivedOn == default(DateOnly))
            return query;

        return query.Where(x => x.ReceivedOn == receivedOn);
    }

    public static IQueryable<PaymentResponse> ApplyPaymentMethodFilter(
        this IQueryable<PaymentResponse> query,
        PaymentMethod? method)
    {
        if (method is null)
            return query;

        return query.Where(x => x.Method == method);
    }

    public static IQueryable<PaymentResponse> ApplyReferenceFilter(
        this IQueryable<PaymentResponse> query,
        string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return query;

        return query.Where(x => x.Reference == reference);
    }


}
