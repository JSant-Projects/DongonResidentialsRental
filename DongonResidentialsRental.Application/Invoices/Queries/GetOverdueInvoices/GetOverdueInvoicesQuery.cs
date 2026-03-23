using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOverdueInvoices;

public sealed record GetOverdueInvoicesQuery(
    LeaseId? LeaseId,
    DateRange? Period,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InvoiceResponse>>;
