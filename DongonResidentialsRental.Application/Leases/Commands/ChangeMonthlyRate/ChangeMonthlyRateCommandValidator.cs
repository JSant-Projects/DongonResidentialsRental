using FluentValidation;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeMonthlyRate;

public sealed class ChangeMonthlyRateCommandValidator : AbstractValidator<ChangeMonthlyRateCommand>
{
    public ChangeMonthlyRateCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();

        RuleFor(x => x.NewMonthlyRate)
            .NotNull()
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);
    }
}