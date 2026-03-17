using FluentValidation;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeLeaseTerm;

public sealed class ChangeLeaseTermCommandValidator : AbstractValidator<ChangeLeaseTermCommand>
{
    public ChangeLeaseTermCommandValidator()
    {
        RuleFor(x => x.NewStartDate)
            .NotEqual(default(DateOnly));
    }
}