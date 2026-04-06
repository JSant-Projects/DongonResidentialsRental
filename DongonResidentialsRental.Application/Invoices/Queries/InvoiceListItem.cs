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


public sealed class InvoiceListRow
{
    public Guid InvoiceId{ get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid LeaseId{ get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string BuildingName{ get; init; } = string.Empty;
    public string UnitNumber{ get; init; } = string.Empty;
    public DateOnly From{ get; init; }
    public DateOnly To{ get; init; }
    public DateOnly DueDate{ get; init; }
    public InvoiceStatus Status{ get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal AmountCredited { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int GracePeriodDays { get; init; }
}
