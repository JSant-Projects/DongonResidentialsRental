namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record GenerateMissingInvoicesRequest(
    DateOnly Today);

