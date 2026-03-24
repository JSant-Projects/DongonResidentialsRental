using FluentValidation;

namespace DongonResidentialsRental.Application.Payments.Commands.ReversePayment;

public sealed class ReversePaymentCommandValidator : AbstractValidator<ReversePaymentCommand>
{
    public ReversePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId.Id)
            .NotEmpty();

        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty();
    }
}