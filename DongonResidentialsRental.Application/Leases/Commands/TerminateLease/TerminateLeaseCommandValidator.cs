using FluentValidation;

namespace DongonResidentialsRental.Application.Leases.Commands.TerminateLease;

public sealed class TerminateLeaseCommandValidator : AbstractValidator<TerminateLeaseCommand>
{
    public TerminateLeaseCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();

        RuleFor(x => x.TerminationDate)
            .NotEqual(default(DateOnly));
    }
}