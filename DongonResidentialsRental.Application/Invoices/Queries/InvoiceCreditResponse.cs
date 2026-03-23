namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoiceCreditResponse(
    string Currency,
    decimal Amount,
    DateOnly AppliedOn);



