using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommand(
    LeaseId LeaseId, 
    DateOnly From, 
    DateOnly To) : ICommand<InvoiceId>;
