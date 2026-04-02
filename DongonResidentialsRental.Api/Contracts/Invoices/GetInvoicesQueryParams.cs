namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record GetInvoicesQueryParams(
    Guid? LeaseId,
    DateOnly? From,
    DateOnly? To,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);

