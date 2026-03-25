using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public static class InvoiceMappings
{
    public static Expression<Func<InvoiceLine, InvoiceLineResponse>> ToInvoiceLineResponse() 
        => invoiceLine => new InvoiceLineResponse(
            invoiceLine.Description,
            invoiceLine.Quantity,
            invoiceLine.UnitPrice.Amount,
            invoiceLine.Type);

    public static Expression<Func<InvoiceAllocation, InvoicePaymentResponse>> ToInvoicePaymentResponse()
        => invoiceAllocation => new InvoicePaymentResponse(
            invoiceAllocation.Amount.Currency,
            invoiceAllocation.Amount.Amount,
            invoiceAllocation.AppliedOn);

    public static Expression<Func<InvoiceCreditAllocation, InvoiceCreditResponse>> ToInvoiceCreditResponse()
        => invoiceCreditAllocation => new InvoiceCreditResponse(
            invoiceCreditAllocation.Amount.Currency,
            invoiceCreditAllocation.Amount.Amount,
            invoiceCreditAllocation.AppliedOn);
}
