namespace DongonResidentialsRental.Api.Contracts.Invoices;

public sealed record GenerateInvoicesForBillingPeriodRequest(
    int Year,
    int Month);

