using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;

public sealed class GenerateInvoicesForBillingPeriodCommandValidator : AbstractValidator<GenerateInvoicesForBillingPeriodCommand>
{
    public GenerateInvoicesForBillingPeriodCommandValidator()
    {
        RuleFor(x => x.Year)
            .NotNull()
            .GreaterThan(1900);

        RuleFor(x => x.Month)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(12);
    }
}