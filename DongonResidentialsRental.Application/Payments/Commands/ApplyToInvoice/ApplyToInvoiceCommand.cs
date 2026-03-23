using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Commands.ApplyToInvoice;

public sealed record ApplyToInvoiceCommand(
    PaymentId PaymentId,
    InvoiceId InvoiceId,
    decimal Amount,
    string Currency) : ICommand<Unit>;
