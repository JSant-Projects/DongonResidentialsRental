using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Payment;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentDetails;

public sealed record GetPaymentDetailsQuery(PaymentId PaymentId) : IQuery<PaymentDetailsResponse>;
