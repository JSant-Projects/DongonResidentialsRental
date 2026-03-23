using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceListItem(
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
    string Currency,
    int GracePeriodDays);
