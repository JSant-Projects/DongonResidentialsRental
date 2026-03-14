using DongonResidentialsRental.Domain.CreditNote;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface ICreditNoteRepository
{
    Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
    Task<CreditNote?> GetByIdAsync(CreditNoteId creditNoteId, CancellationToken cancellationToken = default);
    Task RemoveAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
}
