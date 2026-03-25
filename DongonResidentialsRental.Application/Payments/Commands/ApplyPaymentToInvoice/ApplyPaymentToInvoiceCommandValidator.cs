using FluentValidation;

namespace DongonResidentialsRental.Application.Payments.Commands.ApplyPaymentToInvoice;

public sealed class ApplyPaymentToInvoiceCommandValidator : AbstractValidator<ApplyPaymentToInvoiceCommand>
{
    public ApplyPaymentToInvoiceCommandValidator()
    {
        RuleFor(x => x.PaymentId.Id)
            .NotEmpty();

        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0m);
    }
}
