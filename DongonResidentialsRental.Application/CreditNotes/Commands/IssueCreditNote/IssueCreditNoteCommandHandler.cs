using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandHandler : ICommandHandler<IssueCreditNoteCommand, Unit>
{
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public IssueCreditNoteCommandHandler(
        ICreditNoteRepository creditNoteRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _creditNoteRepository = creditNoteRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        var creditNote = await _creditNoteRepository.GetByIdAsync(request.CreditNoteId);
        if (creditNote is null)
        {
            throw new NotFoundException(nameof(CreditNote), request.CreditNoteId);
        }

        creditNote.Issue(today);

        return Unit.Value;
    }
}
