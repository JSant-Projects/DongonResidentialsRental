using FluentValidation;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandValidator : AbstractValidator<IssueCreditNoteCommand>
{
    public IssueCreditNoteCommandValidator()
    {
        RuleFor(x => x.CreditNoteId.Id)
            .NotEmpty();    
    }
}
