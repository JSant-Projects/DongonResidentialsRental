using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Commands.ReversePayment;

public sealed record ReversePaymentCommand(
    PaymentId PaymentId,
    InvoiceId InvoiceId,
    string Reason) : ICommand<Unit>;
