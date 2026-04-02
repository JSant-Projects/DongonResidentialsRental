using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record AddInvoiceLineRequest(
    string Description,
    int Quantity,
    decimal Price,
    InvoiceLineType LineType);

