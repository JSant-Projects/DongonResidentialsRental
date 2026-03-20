using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceResponse(
    Guid InvoiceId,
    //string InvoiceNumber,
    Guid LeaseId,
    Guid TenantId,
    string TenantName,
    Guid UnitId,
    string UnitNumber,
    DateOnly From,
    DateOnly To,
    DateOnly DueDate,
    string Status,
    decimal TotalAmount,
    decimal Balance,
    string Currency);
