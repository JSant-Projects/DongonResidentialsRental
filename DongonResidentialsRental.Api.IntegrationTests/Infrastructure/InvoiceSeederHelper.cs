using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class InvoiceSeederHelper
{
    public static Invoice CreateInvoice(
        LeaseId leaseId,
        DateOnly from,
        DateOnly to,
        DateOnly dueDate,
        IReadOnlyList<(string Description, int Quantity, decimal Amount, InvoiceLineType LineType)> lineItems,
        string invoiceNumber = "INV-202603-0001",
        string currency = "CAD",
        InvoiceStatus invoiceStatus = InvoiceStatus.Draft)
    {   
        var invoice = Invoice.Create(
            invoiceNumber: invoiceNumber,
            leaseId: leaseId,
            billingPeriod: new BillingPeriod(from, to),
            dueDate: dueDate,
            currency: currency);

        foreach (var (Description, Quantity, Amount, LineType) in lineItems)
        {
            var moneyAmount = Money.Create(currency, Amount);
            invoice.AddLine(Description, Quantity, moneyAmount, LineType);
        }

        switch (invoiceStatus)
        {
            case InvoiceStatus.Issued:
                invoice.Issue(DateOnly.FromDateTime(DateTime.UtcNow));
                break;
            case InvoiceStatus.Cancelled:
                invoice.Cancel();
                break;
        }

        return invoice;
    }

    public static async Task<Invoice> SeedInvoiceAsync(
        IntegrationTestWebAppFactory factory,
        LeaseId leaseId,
        DateOnly from,
        DateOnly to,
        DateOnly dueDate,
        IReadOnlyList<(string Description, int Quantity, decimal Amount, InvoiceLineType LineType)> lineItems,
        string invoiceNumber = "INV-202603-0001",
        string currency = "CAD",
        InvoiceStatus invoiceStatus = InvoiceStatus.Draft)
    {
        var invoice = CreateInvoice(
            leaseId: leaseId,
            from: from,
            to: to,
            dueDate: dueDate,
            lineItems: lineItems,
            invoiceNumber: invoiceNumber,
            currency: currency,
            invoiceStatus: invoiceStatus);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();
        return invoice;
    }
}
