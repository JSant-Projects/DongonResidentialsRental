using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoiceForPayment;

public sealed record OutstandingInvoicesForPaymentResponse(
    Guid InvoiceId,
    string InvoiceNumber,
    DateOnly DueDate,
    decimal Balance,
    string Currency);
