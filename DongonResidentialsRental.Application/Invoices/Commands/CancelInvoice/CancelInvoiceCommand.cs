using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DongonResidentialsRental.Application.Invoices.Commands.CancelInvoice;

public sealed record CancelInvoiceCommand(InvoiceId InvoiceId) : ICommand<Unit>;
