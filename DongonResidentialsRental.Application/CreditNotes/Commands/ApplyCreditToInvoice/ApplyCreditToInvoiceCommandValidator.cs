using FluentValidation;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.ApplyCreditToInvoice;

public sealed class ApplyCreditToInvoiceCommandValidator : AbstractValidator<ApplyCreditToInvoiceCommand>
{
    public ApplyCreditToInvoiceCommandValidator()
    {
        RuleFor(x => x.CreditNoteId.Id)
            .NotEmpty();

        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0m);
    }
}