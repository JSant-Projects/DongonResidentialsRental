namespace DongonResidentialsRental.Application.Invoices.Queries;

public sealed record InvoicePaymentResponse(
    string Currency,
    decimal Amount,
    DateOnly AppliedOn);



