using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;

public sealed record IssueInvoiceCommand(InvoiceId InvoiceId) : ICommand<Unit>;
