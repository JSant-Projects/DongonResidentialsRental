namespace DongonResidentialsRental.Api.Contracts.CreditNotes;

public sealed record RemoveCreditFromInvoiceRequest(
    Guid InvoiceId);
