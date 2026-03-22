using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceListItem(
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
    string Currency);
