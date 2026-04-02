namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record GetOutstandingInvoicesQueryParams(
    Guid? TenantId,
    Guid? LeaseId,
    int Page = 1,
    int PageSize = 20);

