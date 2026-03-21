using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoiceDetails;

public sealed record GetInvoiceDetailsQuery(InvoiceId InvoiceId) : IQuery<InvoiceDetailsResponse>;
