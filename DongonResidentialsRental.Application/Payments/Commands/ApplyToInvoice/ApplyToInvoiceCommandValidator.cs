using FluentValidation;

namespace DongonResidentialsRental.Application.Payments.Commands.ApplyToInvoice;

public sealed class ApplyToInvoiceCommandValidator : AbstractValidator<ApplyToInvoiceCommand>
{
    public ApplyToInvoiceCommandValidator()
    {
        RuleFor(x => x.PaymentId.Id)
            .NotEmpty();

        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);
    }
}
