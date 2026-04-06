namespace DongonResidentialsRental.Api.Contracts.Buildings;

public sealed record GetOutstandingInvoicesForPaymentQueryParams(
    Guid LeaseId);
