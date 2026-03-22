using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceDetailsResponse(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid LeaseId,
    Guid TenantId,
    string TenantName,
    Guid UnitId,
    string UnitNumber,
    DateOnly From,
    DateOnly To,
    DateOnly DueDate,
    InvoiceStatus Status,
    decimal TotalAmount,
    decimal Balance,
    string Currency,
    IReadOnlyList<InvoiceLineResponse> Lines,
    IReadOnlyList<InvoicePaymentResponse> Payments,
    IReadOnlyList<InvoiceCreditResponse> Credits);
