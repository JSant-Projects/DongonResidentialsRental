using FluentValidation;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.VoidCreditNote;

public sealed class VoidCreditNoteCommandValidator : AbstractValidator<VoidCreditNoteCommand>
{
    public VoidCreditNoteCommandValidator()
    {
        RuleFor(x => x.CreditNoteId.Id)
            .NotEmpty();
    }
}