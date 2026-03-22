using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoiceForPayment;

public sealed record GetOutstandingInvoicesForPaymentQuery(
    LeaseId LeaseId) : IQuery<IReadOnlyList<OutstandingInvoicesForPaymentResponse>>;
