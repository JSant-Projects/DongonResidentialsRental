using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.AddInvoiceLine;

public sealed class AddInvoiceLineCommandValidator : AbstractValidator<AddInvoiceLineCommand>
{
    public AddInvoiceLineCommandValidator()
    {
        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .NotNull()
            .GreaterThan(0m);
    }
}