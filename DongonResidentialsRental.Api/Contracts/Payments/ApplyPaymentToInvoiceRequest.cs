using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Contracts.Payments;

public sealed record ApplyPaymentToInvoiceRequest(
    InvoiceId InvoiceId,
    decimal Amount);
