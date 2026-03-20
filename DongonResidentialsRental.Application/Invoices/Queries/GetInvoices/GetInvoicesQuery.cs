using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;

public sealed record GetInvoicesQuery(
    LeaseId? LeaseId,
    TenantId? TenantId,
    DateRange? Period,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InvoiceResponse>>;

