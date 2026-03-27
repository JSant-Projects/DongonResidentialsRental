using FluentValidation;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .MaximumLength(3);
    }
}
