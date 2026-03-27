using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.AddInvoiceLine;

public sealed record AddInvoiceLineCommand(
    InvoiceId InvoiceId,
    string Description,
    int Quantity,
    decimal Price,
    InvoiceLineType LineType) : ICommand<Unit>;
