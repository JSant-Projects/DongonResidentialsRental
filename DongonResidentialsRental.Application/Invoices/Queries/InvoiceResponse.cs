using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceResponse(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid LeaseId,
    string TenantName,
    string BuildingName,
    string UnitNumber,
    DateOnly From,
    DateOnly To,
    DateOnly DueDate,
    InvoiceStatus Status,
    decimal TotalAmount,
    decimal Balance,
    string Currency);
