using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class PaymentSeederHelper
{
    public static Payment CreatePayment(
        TenantId tenantId,
        decimal amount,
        DateOnly receivedOn,
        string currency = "CAD",
        string? reference = "PAY-202603-0001",
        PaymentMethod method = PaymentMethod.Cash)
    {
        return Payment.Create(
            tenantId: tenantId,
            amount: Money.Create(currency, amount),
            receivedOn: receivedOn,
            reference: reference,
            method: method);
    }

    public static async Task<Payment> SeedPaymentAsync(
        IntegrationTestWebAppFactory factory,
        TenantId tenantId,
        decimal amount,
        DateOnly receivedOn,
        string currency = "CAD",
        string? reference = "PAY-202603-0001",
        PaymentMethod method = PaymentMethod.Cash)
    {
        var payment = CreatePayment(
            tenantId: tenantId,
            amount: amount,
            receivedOn: receivedOn,
            currency: currency,
            reference: reference,
            method: method);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Payments.Add(payment);

        await dbContext.SaveChangesAsync();

        return payment;
    }

}
