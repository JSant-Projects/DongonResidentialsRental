using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Commands.ApplyPaymentToInvoice;

public sealed record ApplyPaymentToInvoiceCommand(
    PaymentId PaymentId,
    InvoiceId InvoiceId,
    decimal Amount) : ICommand<Unit>;
