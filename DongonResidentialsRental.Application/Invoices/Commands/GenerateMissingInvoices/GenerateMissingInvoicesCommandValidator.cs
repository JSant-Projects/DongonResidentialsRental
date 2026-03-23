using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;

public sealed class GenerateMissingInvoicesCommandValidator : AbstractValidator<GenerateMissingInvoicesCommand>
{
    public GenerateMissingInvoicesCommandValidator()
    {
        RuleFor(x => x.Today)
            .NotEqual(default(DateOnly));
    }
}