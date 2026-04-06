using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceLineResponse(
    string Description,
    int Quantity,
    decimal Price,
    string Currency,
    InvoiceLineType Type);
