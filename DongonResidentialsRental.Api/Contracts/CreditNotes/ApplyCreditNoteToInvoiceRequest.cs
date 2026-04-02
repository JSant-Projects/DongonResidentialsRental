namespace DongonResidentialsRental.Api.Contracts.CreditNotes;

public sealed record ApplyCreditNoteToInvoiceRequest(
    Guid InvoiceId,
    decimal Amount);
