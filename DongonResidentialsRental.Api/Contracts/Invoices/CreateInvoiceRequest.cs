namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record CreateInvoiceRequest(
    Guid LeaseId,
    DateOnly From,
    DateOnly To);

