using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoicesDueSoon;

public sealed record GetInvoicesDueSoonQuery(
    int Days = 5,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InvoiceResponse>>;
