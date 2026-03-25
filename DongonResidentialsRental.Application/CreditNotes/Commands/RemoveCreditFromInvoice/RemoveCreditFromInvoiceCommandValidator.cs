using FluentValidation;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.RemoveCreditFromInvoice;

public sealed class RemoveCreditFromInvoiceCommandValidator : AbstractValidator<RemoveCreditFromInvoiceCommand>
{
    public RemoveCreditFromInvoiceCommandValidator()
    {
        RuleFor(x => x.CreditNoteId.Id)
            .NotEmpty();

        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();
    }
}