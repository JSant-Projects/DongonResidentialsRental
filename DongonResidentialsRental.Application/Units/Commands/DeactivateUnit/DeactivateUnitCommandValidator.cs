using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.DeactivateUnit;

public sealed class DeactivateUnitCommandValidator : AbstractValidator<DeactivateUnitCommand>
{
    public DeactivateUnitCommandValidator()
    {
        RuleFor(x => x.UnitId.Id)
            .NotEmpty();
    }
}
