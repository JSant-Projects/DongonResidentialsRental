namespace DongonResidentialsRental.Api.Contracts.Buildings;

public sealed record GetInvoicesDueSoonQueryParams(
    int Days = 5,
    int Page = 1,
    int PageSize = 20);
