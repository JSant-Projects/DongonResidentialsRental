using DongonResidentialsRental.Domain.CreditNote;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface ICreditNoteRepository
{
    void Add(CreditNote creditNote);
    Task<CreditNote?> GetByIdAsync(CreditNoteId creditNoteId, CancellationToken cancellationToken = default);
    void Remove(CreditNote creditNote);
}
