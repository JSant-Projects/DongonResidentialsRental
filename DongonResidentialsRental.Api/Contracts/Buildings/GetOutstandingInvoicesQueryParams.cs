using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Contracts.Buildings;

public sealed record GetOutstandingInvoicesQueryParams(
    Guid? TenantId,
    Guid? LeaseId,
    int Page = 1,
    int PageSize = 20);
