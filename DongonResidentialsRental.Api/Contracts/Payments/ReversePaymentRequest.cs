namespace DongonResidentialsRental.Api.Contracts.Payments;

public sealed record ReversePaymentRequest(
    Guid InvoiceId,
    string Reason);
