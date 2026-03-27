using FluentValidation;

namespace DongonResidentialsRental.Application.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.TenantId.Id)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .NotNull()
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(x => x.ReceivedOn)
            .NotEqual(default(DateOnly));

    }
}