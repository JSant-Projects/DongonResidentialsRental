using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.VoidCreditNote;

public sealed class VoidCreditNoteCommandHandler : ICommandHandler<VoidCreditNoteCommand, Unit>
{
    private readonly ICreditNoteRepository _creditNoteRepository;
    public VoidCreditNoteCommandHandler(ICreditNoteRepository creditNoteRepository)
    {
        _creditNoteRepository = creditNoteRepository;
    }
    public async Task<Unit> Handle(VoidCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await _creditNoteRepository.GetByIdAsync(request.CreditNoteId);
        if (creditNote is null)
        {
            throw new NotFoundException(nameof(CreditNote), request.CreditNoteId);
        }

        creditNote.Void(); 

        return Unit.Value;
    }
}
