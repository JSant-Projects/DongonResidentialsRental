using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Commands.PutUnitUnderMaintanance;

public sealed class PutUnitUnderMaintenanceCommandValidator : AbstractValidator<PutUnitUnderMaintenanceCommand>
{
    public PutUnitUnderMaintenanceCommandValidator()
    {
        RuleFor(x => x.UnitId.Id)
            .NotEmpty();
    }
}
