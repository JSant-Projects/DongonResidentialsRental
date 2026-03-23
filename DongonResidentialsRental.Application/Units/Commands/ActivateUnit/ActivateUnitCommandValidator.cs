using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.ActivateUnit;

public sealed class ActivateUnitCommandValidator : AbstractValidator<ActivateUnitCommand>
{
    public ActivateUnitCommandValidator()
    {
        RuleFor(x => x.UnitId.Id)
            .NotEmpty();
    }
}
