namespace DongonResidentialsRental.Api.Contracts.CreditNotes;

public sealed record CreateCreditNoteRequest(
    Guid LeaseId,
    decimal Amount,
    string Currency);
