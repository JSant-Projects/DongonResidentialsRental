using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.CreditNote.Events;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoices;

public sealed record GetOutstandingInvoicesQuery(
    TenantId? TenantId,
    LeaseId? LeaseId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InvoiceResponse>>;
