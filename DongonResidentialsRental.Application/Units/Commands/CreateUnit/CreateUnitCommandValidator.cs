using FluentValidation;

namespace DongonResidentialsRental.Application.Units.Commands.CreateUnit;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(x => x.UnitNumber)
            .NotEmpty()
            .MaximumLength(5);
    }
}
