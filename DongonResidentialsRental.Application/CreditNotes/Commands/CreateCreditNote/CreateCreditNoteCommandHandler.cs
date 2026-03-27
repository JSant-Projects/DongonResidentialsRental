using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandHandler : ICommandHandler<CreateCreditNoteCommand, CreditNoteId>
{
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly ILeaseRepository _leaseRepository;
    public CreateCreditNoteCommandHandler(
        ICreditNoteRepository creditNoteRepository, 
        ILeaseRepository leaseRepository)
    {
        _creditNoteRepository = creditNoteRepository;
        _leaseRepository = leaseRepository;
    }
    public async Task<CreditNoteId> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var creditNoteAmount = Money.Create(request.Currency, request.Amount);

        var creditNote = CreditNote.Create(request.LeaseId, creditNoteAmount);

        _creditNoteRepository.Add(creditNote);

        return creditNote.CreditNoteId;
    }
}
