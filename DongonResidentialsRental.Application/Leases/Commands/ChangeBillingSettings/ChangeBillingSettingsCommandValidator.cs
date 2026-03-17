using FluentValidation;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeBillingSettings;

public sealed class ChangeBillingSettingsCommandValidator : AbstractValidator<ChangeBillingSettingsCommand>
{
    public ChangeBillingSettingsCommandValidator()
    {
        RuleFor(x => x.NewDueDayOfMonth)
            .NotNull()
            .GreaterThan(0);

        RuleFor(x => x.NewGracePeriodDays)
            .NotNull()
            .GreaterThan(0);
    }
}