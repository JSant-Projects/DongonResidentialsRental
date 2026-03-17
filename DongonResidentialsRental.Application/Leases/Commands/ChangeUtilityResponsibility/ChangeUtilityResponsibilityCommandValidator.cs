using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeUtilityResponsibility;

public class ChangeUtilityResponsibilityCommandValidator : AbstractValidator<ChangeUtilityResponsibilityCommand>
{
    public ChangeUtilityResponsibilityCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();
    }
}
