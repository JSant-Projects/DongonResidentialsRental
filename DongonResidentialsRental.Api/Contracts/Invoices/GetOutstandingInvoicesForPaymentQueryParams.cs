using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record GetOutstandingInvoicesForPaymentQueryParams(
    Guid LeaseId);

